using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerCrowned.DTOs.PreOrdenTrabajo;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PreOrdenTrabajoController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public PreOrdenTrabajoController(
            dbContext context,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? estado,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();
                if (!await IsPreOrderModuleEnabled(workshopId.Value)) return PreOrderModuleDisabled();

                var query = _context.PreOrdenesTrabajo
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(x =>
                        x.Cliente.ToLower().Contains(s) ||
                        x.Matricula.ToLower().Contains(s) ||
                        x.Modelo.ToLower().Contains(s) ||
                        x.MotivoRecepcion.ToLower().Contains(s) ||
                        (x.Marca != null && x.Marca.ToLower().Contains(s))
                    );
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    var e = estado.Trim().ToLower();
                    query = query.Where(x => x.Estado.ToLower() == e);
                }

                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(x => x.Fecha)
                    .ThenByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                foreach (var item in items)
                {
                    await EnrichCustomerDataAsync(item, workshopId.Value);
                }

                respuesta.Ok = 1;
                respuesta.Message = "Pre-ordenes";
                respuesta.Data.Add(new { items, total, page, pageSize });
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();
                if (!await IsPreOrderModuleEnabled(workshopId.Value)) return PreOrderModuleDisabled();

                var item = await _context.PreOrdenesTrabajo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (item == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe la pre-orden o fue eliminada.";
                    return NotFound(respuesta);
                }

                await EnrichCustomerDataAsync(item, workshopId.Value);

                respuesta.Ok = 1;
                respuesta.Message = "Pre-orden encontrada";
                respuesta.Data.Add(item);
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
        public async Task<ActionResult> Create([FromBody] PreOrdenTrabajoCreateDto dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Cliente))
                    return BadRequest(new { message = "El cliente es requerido." });
                if (string.IsNullOrWhiteSpace(dto.Matricula))
                    return BadRequest(new { message = "La matricula es requerida." });
                if (string.IsNullOrWhiteSpace(dto.Modelo))
                    return BadRequest(new { message = "El modelo es requerido." });
                if (string.IsNullOrWhiteSpace(dto.MotivoRecepcion))
                    return BadRequest(new { message = "El motivo recibido es requerido." });

                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();
                if (!await IsPreOrderModuleEnabled(workshopId.Value)) return PreOrderModuleDisabled();

                var item = new PreOrdenTrabajo
                {
                    Cliente = dto.Cliente.Trim(),
                    Dni = dto.Dni?.Trim(),
                    Telefono = dto.Telefono?.Trim(),
                    Direccion = dto.Direccion?.Trim(),
                    Matricula = dto.Matricula.Trim().ToUpper(),
                    Marca = dto.Marca?.Trim(),
                    Modelo = dto.Modelo.Trim(),
                    Kilometraje = dto.Kilometraje,
                    Fecha = dto.Fecha == default ? DateTime.Now : dto.Fecha,
                    FechaPrevistaEntrega = dto.FechaPrevistaEntrega,
                    TiempoEstimadoHoras = dto.TiempoEstimadoHoras,
                    MotivoRecepcion = dto.MotivoRecepcion.Trim(),
                    DiagnosticoMecanico = dto.DiagnosticoMecanico?.Trim(),
                    RepuestosNecesarios = dto.RepuestosNecesarios?.Trim(),
                    Observaciones = dto.Observaciones?.Trim(),
                    Estado = "Pendiente",
                    Eliminado = false
                };

                await EnrichCustomerDataAsync(item, workshopId.Value);

                _context.PreOrdenesTrabajo.Add(item);
                _context.Entry(item).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Pre-orden registrada correctamente.";
                respuesta.Data.Add(item);
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] PreOrdenTrabajoUpdateDto dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();
                if (!await IsPreOrderModuleEnabled(workshopId.Value)) return PreOrderModuleDisabled();

                var item = await _context.PreOrdenesTrabajo
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (item == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe la pre-orden o fue eliminada.";
                    return NotFound(respuesta);
                }

                if (item.ConvertidaEnOrden)
                    return BadRequest(new { message = "No se puede editar una pre-orden ya convertida." });

                if (!string.IsNullOrWhiteSpace(dto.Cliente)) item.Cliente = dto.Cliente.Trim();
                if (dto.Dni != null) item.Dni = dto.Dni.Trim();
                if (dto.Telefono != null) item.Telefono = dto.Telefono.Trim();
                if (dto.Direccion != null) item.Direccion = dto.Direccion.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Matricula)) item.Matricula = dto.Matricula.Trim().ToUpper();
                if (dto.Marca != null) item.Marca = dto.Marca.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Modelo)) item.Modelo = dto.Modelo.Trim();
                if (dto.Kilometraje.HasValue) item.Kilometraje = dto.Kilometraje.Value;
                if (dto.Fecha.HasValue) item.Fecha = dto.Fecha.Value;
                item.FechaPrevistaEntrega = dto.FechaPrevistaEntrega;
                item.TiempoEstimadoHoras = dto.TiempoEstimadoHoras;
                if (!string.IsNullOrWhiteSpace(dto.MotivoRecepcion)) item.MotivoRecepcion = dto.MotivoRecepcion.Trim();
                if (dto.DiagnosticoMecanico != null) item.DiagnosticoMecanico = dto.DiagnosticoMecanico.Trim();
                if (dto.RepuestosNecesarios != null) item.RepuestosNecesarios = dto.RepuestosNecesarios.Trim();
                if (dto.Observaciones != null) item.Observaciones = dto.Observaciones.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Estado)) item.Estado = dto.Estado.Trim();

                await EnrichCustomerDataAsync(item, workshopId.Value);

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Pre-orden actualizada correctamente.";
                respuesta.Data.Add(item);
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpPut("{id:int}/convertida")]
        public async Task<ActionResult> MarkConverted(int id, [FromBody] PreOrdenTrabajoConvertDto dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();
                if (!await IsPreOrderModuleEnabled(workshopId.Value)) return PreOrderModuleDisabled();

                var item = await _context.PreOrdenesTrabajo
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (item == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe la pre-orden o fue eliminada.";
                    return NotFound(respuesta);
                }

                var orderExists = await _context.OrdenesTrabajo.AnyAsync(x =>
                    x.Id == dto.IdOrdenTrabajo &&
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value
                );

                if (!orderExists)
                    return BadRequest(new { message = "La orden indicada no existe." });

                item.ConvertidaEnOrden = true;
                item.IdOrdenTrabajo = dto.IdOrdenTrabajo;
                item.Estado = "Convertida";
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Pre-orden convertida correctamente.";
                respuesta.Data.Add(new { item.Id, item.IdOrdenTrabajo });
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
        public async Task<ActionResult> Delete(int id)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();
                if (!await IsPreOrderModuleEnabled(workshopId.Value)) return PreOrderModuleDisabled();

                var item = await _context.PreOrdenesTrabajo
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (item == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe la pre-orden o ya fue eliminada.";
                    return NotFound(respuesta);
                }

                item.Eliminado = true;
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Pre-orden eliminada correctamente.";
                respuesta.Data.Add(new { item.Id });
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        private async Task<bool> IsPreOrderModuleEnabled(int workshopId)
        {
            return await _context.Workshops
                .AsNoTracking()
                .Where(x => x.Id == workshopId && x.Activo)
                .Select(x => x.EnablePreOrders)
                .FirstOrDefaultAsync();
        }

        private ObjectResult PreOrderModuleDisabled()
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "El modulo de pre-ordenes no esta habilitado para este taller."
            });
        }

        private async Task EnrichCustomerDataAsync(PreOrdenTrabajo item, int workshopId)
        {
            if (item == null) return;

            var needsDni = string.IsNullOrWhiteSpace(item.Dni);
            var needsAddress = string.IsNullOrWhiteSpace(item.Direccion);
            if (!needsDni && !needsAddress) return;

            var matricula = item.Matricula?.Trim().ToUpper();
            var telefono = item.Telefono?.Trim();
            var cliente = item.Cliente?.Trim().ToLower();

            var match = await _context.Clientes
                .AsNoTracking()
                .Where(x =>
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId &&
                    (
                        (!string.IsNullOrEmpty(matricula) && x.Matricula.ToUpper() == matricula) ||
                        (!string.IsNullOrEmpty(telefono) && x.Telefono == telefono) ||
                        (!string.IsNullOrEmpty(cliente) && x.Nombre.ToLower() == cliente)
                    )
                )
                .OrderByDescending(x => !string.IsNullOrEmpty(matricula) && x.Matricula.ToUpper() == matricula)
                .ThenByDescending(x => !string.IsNullOrEmpty(telefono) && x.Telefono == telefono)
                .FirstOrDefaultAsync();

            if (match == null) return;

            if (needsDni) item.Dni = match.Dni;
            if (string.IsNullOrWhiteSpace(item.Telefono)) item.Telefono = match.Telefono;
            if (needsAddress) item.Direccion = match.Direccion;
        }
    }

}
