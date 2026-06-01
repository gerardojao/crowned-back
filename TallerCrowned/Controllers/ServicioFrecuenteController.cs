using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ServicioFrecuenteController : ControllerBase
    {
        private static readonly string[] DefaultServices =
        {
            "Servicio cambio de aceite y filtro",
            "Cambio de pastillas de frenos",
            "Cambio de rodamientos delanteros",
            "Cambio de amortiguadores",
            "Mano de obra",
            "Repuestos"
        };

        private readonly dbContext _context;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public ServicioFrecuenteController(
            dbContext context,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                await EnsureDefaults(workshopId.Value);

                var servicios = await _context.ServiciosFrecuentes
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    )
                    .OrderBy(x => x.Nombre)
                    .Select(x => new
                    {
                        x.Id,
                        x.Nombre
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Servicios frecuentes";
                respuesta.Data.Add(servicios);
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ServicioFrecuenteCreateDto dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { message = "El nombre del servicio es requerido." });

                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var nombre = NormalizeName(dto.Nombre);

                var existente = await _context.ServiciosFrecuentes
                    .FirstOrDefaultAsync(x =>
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value &&
                        x.Nombre.ToLower() == nombre.ToLower()
                    );

                if (existente != null)
                {
                    if (existente.Eliminado)
                    {
                        existente.Eliminado = false;
                        existente.FechaEliminacion = null;
                        await _context.SaveChangesAsync();
                    }

                    respuesta.Ok = 1;
                    respuesta.Message = "El servicio ya estaba registrado.";
                    respuesta.Data.Add(new { existente.Id, existente.Nombre });
                    return Ok(respuesta);
                }

                var servicio = new ServicioFrecuente
                {
                    Nombre = nombre,
                    Eliminado = false
                };

                _context.ServiciosFrecuentes.Add(servicio);
                _context.Entry(servicio).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Servicio registrado correctamente.";
                respuesta.Data.Add(new { servicio.Id, servicio.Nombre });
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteSoft(int id)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var servicio = await _context.ServiciosFrecuentes
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (servicio == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el servicio o ya fue eliminado.";
                    return NotFound(respuesta);
                }

                servicio.Eliminado = true;
                servicio.FechaEliminacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Servicio eliminado correctamente.";
                respuesta.Data.Add(new { servicio.Id });
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        private async Task EnsureDefaults(int workshopId)
        {
            var existingCount = await _context.ServiciosFrecuentes
                .CountAsync(x =>
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId
                );

            if (existingCount > 0)
                return;

            foreach (var service in DefaultServices)
            {
                var servicio = new ServicioFrecuente
                {
                    Nombre = service,
                    Eliminado = false
                };

                _context.ServiciosFrecuentes.Add(servicio);
                _context.Entry(servicio).Property("WorkshopId").CurrentValue = workshopId;
            }

            await _context.SaveChangesAsync();
        }

        private static string NormalizeName(string value)
            => string.Join(" ", value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public class ServicioFrecuenteCreateDto
    {
        public string Nombre { get; set; } = "";
    }
}
