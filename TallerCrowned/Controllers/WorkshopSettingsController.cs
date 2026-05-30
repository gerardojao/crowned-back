using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkshopSettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WorkshopSettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<WorkshopSettingsDto> Get()
        {
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
                SerieFactura = section["SerieFactura"] ?? "A"
            });
        }
    }

    public class WorkshopSettingsDto
    {
        public string Nombre { get; set; } = null!;
        public string RazonSocial { get; set; } = null!;
        public string Nif { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Iban { get; set; } = null!;
        public string SerieFactura { get; set; } = "A";
    }
}
