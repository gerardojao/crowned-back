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
    public class AlertaClienteController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public AlertaClienteController(
            dbContext context,
            ICurrentUserService currentUserService,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpGet("pendientes")]
        public async Task<ActionResult> GetPendientes()
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();
                var ahora = DateTime.UtcNow;

                var alertas = await _context.AlertasClientes
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        !x.Atendida &&
                        x.FechaAviso <= ahora &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    )
                    .OrderBy(x => x.FechaAviso)
                    .Select(x => new
                    {
                        x.Id,
                        x.Cliente,
                        x.Telefono,
                        x.Mensaje,
                        x.FechaAviso,
                        x.IdFacturaEmitida
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Alertas pendientes";
                respuesta.Data.Add(alertas);

                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpPut("{id:int}/atendida")]
        public async Task<ActionResult> MarcarAtendida(int id)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var alerta = await _context.AlertasClientes
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (alerta == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe la alerta.";
                    return NotFound(respuesta);
                }

                alerta.Atendida = true;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Alerta marcada como atendida.";
                respuesta.Data.Add(new
                {
                    alerta.Id,
                    alerta.Atendida
                });

                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }
    }
}
