using FamilyApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkshopBankAccountsController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public WorkshopBankAccountsController(
            dbContext context,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpGet]
        public async Task<ActionResult> GetCurrentWorkshopBanks()
        {
            var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
            if (!workshopId.HasValue) return Forbid();

            await EnsureLegacyBankExists(workshopId.Value);
            return Ok(await GetBanks(workshopId.Value, activeOnly: true));
        }

        [Authorize(Roles = "superadmin")]
        [HttpGet("admin/{workshopId:int}")]
        public async Task<ActionResult> GetAdminBanks(int workshopId)
        {
            if (!await _context.Workshops.AnyAsync(x => x.Id == workshopId))
                return NotFound(new { message = "No existe el negocio." });

            await EnsureLegacyBankExists(workshopId);
            return Ok(await GetBanks(workshopId, activeOnly: false));
        }

        [Authorize(Roles = "superadmin")]
        [HttpPost("admin/{workshopId:int}")]
        public async Task<ActionResult> Create(int workshopId, [FromBody] WorkshopBankAccountDto dto)
        {
            var workshop = await _context.Workshops.FirstOrDefaultAsync(x => x.Id == workshopId);
            if (workshop == null) return NotFound(new { message = "No existe el negocio." });
            if (string.IsNullOrWhiteSpace(dto.Iban)) return BadRequest(new { message = "El IBAN es requerido." });

            var hasActive = await _context.WorkshopBankAccounts.AnyAsync(x => x.WorkshopId == workshopId && x.Activo);
            var bank = new WorkshopBankAccount
            {
                WorkshopId = workshopId,
                Nombre = CleanName(dto.Nombre),
                Iban = dto.Iban.Trim(),
                EsPrincipal = dto.EsPrincipal || !hasActive,
                Activo = dto.Activo ?? true,
                FechaCreacion = DateTime.UtcNow
            };

            if (bank.EsPrincipal)
                await ClearMainBank(workshopId);

            _context.WorkshopBankAccounts.Add(bank);
            await _context.SaveChangesAsync();
            await SyncWorkshopIban(workshopId);

            return Ok(await GetBanks(workshopId, activeOnly: false));
        }

        [Authorize(Roles = "superadmin")]
        [HttpPut("admin/{workshopId:int}/{id:int}")]
        public async Task<ActionResult> Update(int workshopId, int id, [FromBody] WorkshopBankAccountDto dto)
        {
            var bank = await _context.WorkshopBankAccounts
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkshopId == workshopId);
            if (bank == null) return NotFound(new { message = "No existe el banco." });
            if (string.IsNullOrWhiteSpace(dto.Iban)) return BadRequest(new { message = "El IBAN es requerido." });

            bank.Nombre = CleanName(dto.Nombre);
            bank.Iban = dto.Iban.Trim();
            if (dto.Activo.HasValue) bank.Activo = dto.Activo.Value;

            if (dto.EsPrincipal)
            {
                await ClearMainBank(workshopId, exceptId: bank.Id);
                bank.EsPrincipal = true;
                bank.Activo = true;
            }
            else if (!bank.Activo)
            {
                bank.EsPrincipal = false;
            }

            await _context.SaveChangesAsync();
            await EnsureOneMainBank(workshopId);
            await SyncWorkshopIban(workshopId);

            return Ok(await GetBanks(workshopId, activeOnly: false));
        }

        [Authorize(Roles = "superadmin")]
        [HttpDelete("admin/{workshopId:int}/{id:int}")]
        public async Task<ActionResult> Deactivate(int workshopId, int id)
        {
            var bank = await _context.WorkshopBankAccounts
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkshopId == workshopId);
            if (bank == null) return NotFound(new { message = "No existe el banco." });

            bank.Activo = false;
            bank.EsPrincipal = false;
            await _context.SaveChangesAsync();
            await EnsureOneMainBank(workshopId);
            await SyncWorkshopIban(workshopId);

            return Ok(await GetBanks(workshopId, activeOnly: false));
        }

        private async Task<List<WorkshopBankAccountResponseDto>> GetBanks(int workshopId, bool activeOnly)
        {
            var query = _context.WorkshopBankAccounts
                .AsNoTracking()
                .Where(x => x.WorkshopId == workshopId);

            if (activeOnly)
                query = query.Where(x => x.Activo);

            return await query
                .OrderByDescending(x => x.EsPrincipal)
                .ThenBy(x => x.Nombre)
                .Select(x => new WorkshopBankAccountResponseDto
                {
                    Id = x.Id,
                    WorkshopId = x.WorkshopId,
                    Nombre = x.Nombre,
                    Iban = x.Iban,
                    EsPrincipal = x.EsPrincipal,
                    Activo = x.Activo
                })
                .ToListAsync();
        }

        private async Task EnsureLegacyBankExists(int workshopId)
        {
            if (await _context.WorkshopBankAccounts.AnyAsync(x => x.WorkshopId == workshopId))
                return;

            var workshop = await _context.Workshops.FirstOrDefaultAsync(x => x.Id == workshopId);
            if (workshop == null || string.IsNullOrWhiteSpace(workshop.Iban))
                return;

            _context.WorkshopBankAccounts.Add(new WorkshopBankAccount
            {
                WorkshopId = workshopId,
                Nombre = "Cuenta principal",
                Iban = workshop.Iban.Trim(),
                EsPrincipal = true,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private async Task ClearMainBank(int workshopId, int? exceptId = null)
        {
            var banks = await _context.WorkshopBankAccounts
                .Where(x => x.WorkshopId == workshopId && (!exceptId.HasValue || x.Id != exceptId.Value))
                .ToListAsync();

            foreach (var item in banks)
                item.EsPrincipal = false;
        }

        private async Task EnsureOneMainBank(int workshopId)
        {
            var hasMain = await _context.WorkshopBankAccounts
                .AnyAsync(x => x.WorkshopId == workshopId && x.Activo && x.EsPrincipal);
            if (hasMain) return;

            var firstActive = await _context.WorkshopBankAccounts
                .Where(x => x.WorkshopId == workshopId && x.Activo)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
            if (firstActive != null)
                firstActive.EsPrincipal = true;

            await _context.SaveChangesAsync();
        }

        private async Task SyncWorkshopIban(int workshopId)
        {
            var workshop = await _context.Workshops.FirstOrDefaultAsync(x => x.Id == workshopId);
            if (workshop == null) return;

            var main = await _context.WorkshopBankAccounts
                .AsNoTracking()
                .Where(x => x.WorkshopId == workshopId && x.Activo)
                .OrderByDescending(x => x.EsPrincipal)
                .ThenBy(x => x.Id)
                .FirstOrDefaultAsync();

            workshop.Iban = main?.Iban;
            await _context.SaveChangesAsync();
        }

        private static string CleanName(string? value)
        {
            var name = string.IsNullOrWhiteSpace(value) ? "Cuenta bancaria" : value.Trim();
            return name.Length > 120 ? name[..120] : name;
        }
    }

    public class WorkshopBankAccountDto
    {
        public string? Nombre { get; set; }
        public string? Iban { get; set; }
        public bool EsPrincipal { get; set; }
        public bool? Activo { get; set; }
    }

    public class WorkshopBankAccountResponseDto
    {
        public int Id { get; set; }
        public int WorkshopId { get; set; }
        public string Nombre { get; set; } = "";
        public string Iban { get; set; } = "";
        public bool EsPrincipal { get; set; }
        public bool Activo { get; set; }
    }
}
