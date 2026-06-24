

using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerCrowned.Models;

namespace FamilyApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FichaIngresoController : ControllerBase
    {

        private readonly IRepository _repository;
        private readonly IWebHostEnvironment _env;
        private readonly dbContext _context;
        private readonly ICurrentWorkshopService _currentWorkshopService;


        public FichaIngresoController(
            IRepository repository,
            IWebHostEnvironment env,
            dbContext context,
            ICurrentWorkshopService currentWorkshopService)
        {
           _repository = repository;
            _env = env;
            _context = context;
            _currentWorkshopService = currentWorkshopService;
     
        }

        // GET: api/SolicitudFichaIngresos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FichaIngreso>>> GetAllIngresos()
        {
            Respuesta<object> respuesta = new();
            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var fIngresos = await _context.FichaIngresos
                    .AsNoTracking()
                    .Where(x => EF.Property<int>(x, "WorkshopId") == workshopId.Value)
                    .ToListAsync();
                if (fIngresos != null)
                {
                    foreach (var item in fIngresos)
                    {
                        respuesta.Data.Add(item);
                    }
                    respuesta.Ok = 1;
                    respuesta.Message = "Ingresos Registrados";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No hay ingresos registrados";
                }

            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);
        }

        //POST: api/FichaEgreso
        [HttpPost("Create")]
        public async Task<ActionResult> PostFichaIngreso(FichaIngreso fichaIngreso)
        {
            Respuesta<object> respuesta = new();
            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var tipoExiste = await _context.Ingresos.AnyAsync(x =>
                    x.Id == fichaIngreso.NombreIngreso &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value);
                if (!tipoExiste)
                    return BadRequest(new { message = "El tipo de ingreso no pertenece al taller activo." });

                // saneo básico (si lo necesitas)
                var bank = await ResolveBankAccount(workshopId.Value, fichaIngreso.BankAccountId);
                fichaIngreso.BankAccountId = bank?.Id;
                fichaIngreso.BankAccountName = bank?.Nombre;
                fichaIngreso.BankAccountIban = bank?.Iban;
                fichaIngreso.Eliminado = false;
                fichaIngreso.FechaEliminacion = null;

                _context.FichaIngresos.Add(fichaIngreso);
                _context.Entry(fichaIngreso).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Success";
                respuesta.Data.Add(new { fichaIngreso.Id });
                return Ok(respuesta);

            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
                return Ok(respuesta);
            }
           
        }
      

        private async Task<WorkshopBankAccount?> ResolveBankAccount(int workshopId, int? bankAccountId)
        {
            IQueryable<WorkshopBankAccount> query = _context.WorkshopBankAccounts
                .AsNoTracking()
                .Where(x => x.WorkshopId == workshopId && x.Activo);

            WorkshopBankAccount? bank = null;
            if (bankAccountId.HasValue)
            {
                bank = await query.FirstOrDefaultAsync(x => x.Id == bankAccountId.Value);
                if (bank == null)
                    throw new ArgumentException("El banco seleccionado no pertenece al taller activo.");
            }

            bank ??= await query
                .OrderByDescending(x => x.EsPrincipal)
                .ThenBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (bank != null)
                return bank;

            var workshop = await _context.Workshops.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workshopId);
            if (workshop == null || string.IsNullOrWhiteSpace(workshop.Iban))
                return null;

            return new WorkshopBankAccount
            {
                WorkshopId = workshopId,
                Nombre = "Cuenta principal",
                Iban = workshop.Iban.Trim(),
                EsPrincipal = true,
                Activo = true
            };
        }
    }
}
