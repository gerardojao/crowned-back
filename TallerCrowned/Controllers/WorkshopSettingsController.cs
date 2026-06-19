using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FamilyApp.Data;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkshopSettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public WorkshopSettingsController(
            IConfiguration configuration,
            dbContext context,
            ICurrentUserService currentUserService,
            ICurrentWorkshopService currentWorkshopService)
        {
            _configuration = configuration;
            _context = context;
            _currentUserService = currentUserService;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpGet]
        public async Task<ActionResult<WorkshopSettingsDto>> Get()
        {
            var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();

            if (workshopId.HasValue)
            {
                var workshop = await _context.Workshops
                    .AsNoTracking()
                    .Where(x => x.Id == workshopId.Value && x.Activo)
                    .FirstOrDefaultAsync();

                if (workshop != null)
                    return Ok(ToDto(workshop));
            }

            var firstWorkshop = await _context.Workshops
                .AsNoTracking()
                .Where(x => x.Activo)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (firstWorkshop != null)
                return Ok(ToDto(firstWorkshop));

            var section = _configuration.GetSection("Workshop");

            return Ok(new WorkshopSettingsDto
            {
                Nombre = section["Nombre"] ?? "Multiservicios Crower",
                RazonSocial = section["RazonSocial"] ?? "JUAN CARLOS FERNANDEZ SILVA",
                Nif = section["Nif"] ?? "61407055E",
                Direccion = section["Direccion"] ?? "CALLE ALCACER 63 D, Albal, 46470",
                Telefono = section["Telefono"] ?? "960057935/655042253",
                Email = section["Email"] ?? "multiservicioscrower@gmail.com",
                Iban = section["Iban"] ?? "ES69 2100 4014 9122 0012 3843",
                SerieFactura = section["SerieFactura"] ?? "A",
                LogoPath = section["LogoPath"],
                LogoUrl = ToLogoUrl(section["LogoPath"]),
                BusinessType = section["BusinessType"] ?? "automotive",
                TerminologyProfile = section["TerminologyProfile"] ?? "automotive",
                MaxUsers = 3,
                FooterText = "© App Multitaller. Todos los derechos reservados.",
                PrivacyPolicyText = null,
                TermsText = null,
                EnableWhatsappAlerts = true,
                EnableInvoiceExport = true,
                EnableProfitAndLoss = true,
                EnableDashboardRepairVehicles = true,
                EnableAccountsReceivable = true
            });
        }

        [HttpGet("mine")]
        public async Task<ActionResult> GetMine()
        {
            var uid = _currentUserService.UserIdInt;
            if (!uid.HasValue)
                return Unauthorized();

            if (User.IsInRole("superadmin"))
            {
                var allWorkshops = await _context.Workshops
                    .AsNoTracking()
                    .Where(x => x.Activo)
                    .OrderBy(x => x.Nombre)
                    .Select(x => ToDto(x))
                    .ToListAsync();

                return Ok(allWorkshops);
            }

            var workshops = await _context.WorkshopUsers
                .AsNoTracking()
                .Where(x => x.UserId == uid.Value && x.Activo && x.Workshop.Activo)
                .OrderBy(x => x.Workshop.Nombre)
                .Select(x => ToDto(x.Workshop))
                .ToListAsync();

            return Ok(workshops);
        }

        private static WorkshopSettingsDto ToDto(TallerCrowned.Models.Workshop workshop)
        {
            return new WorkshopSettingsDto
            {
                Id = workshop.Id,
                Nombre = workshop.Nombre,
                RazonSocial = workshop.RazonSocial,
                Nif = workshop.Nif,
                Direccion = workshop.Direccion,
                Telefono = workshop.Telefono ?? "",
                Email = workshop.Email ?? "",
                Iban = workshop.Iban ?? "",
                SerieFactura = workshop.SerieFactura,
                LogoPath = workshop.LogoPath,
                LogoUrl = ToLogoUrl(workshop.LogoPath),
                BusinessType = string.IsNullOrWhiteSpace(workshop.BusinessType) ? "automotive" : workshop.BusinessType,
                TerminologyProfile = string.IsNullOrWhiteSpace(workshop.TerminologyProfile) ? "automotive" : workshop.TerminologyProfile,
                MaxUsers = workshop.MaxUsers,
                FooterText = workshop.FooterText,
                PrivacyPolicyText = workshop.PrivacyPolicyText,
                TermsText = workshop.TermsText,
                EnableWhatsappAlerts = workshop.EnableWhatsappAlerts,
                EnableInvoiceExport = workshop.EnableInvoiceExport,
                EnableProfitAndLoss = workshop.EnableProfitAndLoss,
                EnableDashboardRepairVehicles = workshop.EnableDashboardRepairVehicles,
                EnableAccountsReceivable = workshop.EnableAccountsReceivable
            };
        }

        private static string? ToLogoUrl(string? logoPath)
        {
            if (string.IsNullOrWhiteSpace(logoPath))
                return null;

            return logoPath.StartsWith("/")
                ? logoPath
                : $"/{logoPath.TrimStart('/')}";
        }
    }

    public class WorkshopSettingsDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string RazonSocial { get; set; } = null!;
        public string Nif { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Iban { get; set; } = null!;
        public string SerieFactura { get; set; } = "A";
        public string? LogoPath { get; set; }
        public string? LogoUrl { get; set; }
        public string BusinessType { get; set; } = "automotive";
        public string TerminologyProfile { get; set; } = "automotive";
        public int MaxUsers { get; set; } = 3;
        public string? FooterText { get; set; }
        public string? PrivacyPolicyText { get; set; }
        public string? TermsText { get; set; }
        public bool EnableWhatsappAlerts { get; set; } = true;
        public bool EnableInvoiceExport { get; set; } = true;
        public bool EnableProfitAndLoss { get; set; } = true;
        public bool EnableDashboardRepairVehicles { get; set; } = true;
        public bool EnableAccountsReceivable { get; set; } = true;
    }
}
