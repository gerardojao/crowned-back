using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerCrowned.DTOs.OrdenTrabajo;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
public class OrdenTrabajoController : Controller
    {
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public OrdenTrabajoController(
            dbContext context,
            ICurrentUserService currentUserService,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _currentWorkshopService = currentWorkshopService;
        }

        private static bool IsEditLocked(string? estado)
        {
            var normalized = (estado ?? "").Trim().ToLowerInvariant();
            return normalized is "reparando" or "esperando repuesto" or "listo" or "entregado";
        }

        private static decimal NormalizeCantidad(decimal cantidad)
        {
            return cantidad <= 0 ? 1 : cantidad;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] OrdenTrabajoSearchDto filter)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var query = _context.OrdenesTrabajo
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (!string.IsNullOrWhiteSpace(filter.Matricula))
                {
                    var matricula = filter.Matricula.Trim().ToLower();
                    query = query.Where(x => x.Matricula.ToLower().Contains(matricula));
                }

                if (filter.FechaDesde.HasValue)
                {
                    var desde = filter.FechaDesde.Value.Date;
                    query = query.Where(x => x.Fecha >= desde);
                }

                if (filter.FechaHasta.HasValue)
                {
                    var hasta = filter.FechaHasta.Value.Date.AddDays(1);
                    query = query.Where(x => x.Fecha < hasta);
                }

                if (!string.IsNullOrWhiteSpace(filter.Cliente))
                {
                    var cliente = filter.Cliente.Trim().ToLower();
                    query = query.Where(x => x.Cliente.ToLower().Contains(cliente));
                }

                if (!string.IsNullOrWhiteSpace(filter.Estado))
                {
                    var estado = filter.Estado.Trim().ToLower();
                    query = query.Where(x => x.Estado.ToLower() == estado);
                }

                var page = filter.Page <= 0 ? 1 : filter.Page;
                var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
                if (pageSize > 100) pageSize = 100;

                var total = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.Fecha)
                    .ThenByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new OrdenTrabajo
                    {
                        Id = x.Id,
                        Cliente = x.Cliente,
                        Dni = x.Dni,
                        Telefono = x.Telefono,
                        Matricula = x.Matricula,
                        Marca = x.Marca,
                        Modelo = x.Modelo,
                        Kilometraje = x.Kilometraje,
                        Fecha = x.Fecha,
                        Trabajo = x.Trabajo,
                        ItemsJson = x.ItemsJson,
                        Repuestos = x.Repuestos,
                        Cantidad = x.Cantidad,
                        ManoObra = x.ManoObra,
                        Estado = x.Estado,
                        Observaciones = x.Observaciones,
                        Facturada = x.Facturada
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Órdenes de trabajo";
                respuesta.Data.Add(new
                {
                    items = data,
                    total,
                    page,
                    pageSize
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

        [HttpGet("ultimas")]
        public async Task<ActionResult> GetUltimas([FromQuery] int take = 10)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                if (take <= 0) take = 10;
                if (take > 100) take = 100;

                var data = await _context.OrdenesTrabajo
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    )
                    .OrderByDescending(x => x.Fecha)
                    .ThenByDescending(x => x.Id)
                    .Take(take)
                    .Select(x => new OrdenTrabajo
                    {
                        Id = x.Id,
                        Cliente = x.Cliente,
                        Dni = x.Dni,
                        Telefono = x.Telefono,
                        Matricula = x.Matricula,
                        Marca = x.Marca,
                        Modelo = x.Modelo,
                        Kilometraje = x.Kilometraje,
                        Fecha = x.Fecha,
                        Trabajo = x.Trabajo,
                        ItemsJson = x.ItemsJson,
                        Repuestos = x.Repuestos,
                        Cantidad = x.Cantidad,
                        ManoObra = x.ManoObra,
                        Estado = x.Estado,
                        Observaciones = x.Observaciones,
                        Facturada = x.Facturada
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Últimas órdenes";
                respuesta.Data.Add(data);

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

                var orden = await _context.OrdenesTrabajo
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    )
                    .Select(x => new OrdenTrabajo
                    {
                        Id = x.Id,
                        Cliente = x.Cliente,
                        Dni = x.Dni,
                        Telefono = x.Telefono,
                        Matricula = x.Matricula,
                        Marca = x.Marca,
                        Modelo = x.Modelo,
                        Kilometraje = x.Kilometraje,
                        Fecha = x.Fecha,
                        Trabajo = x.Trabajo,
                        ItemsJson = x.ItemsJson,
                        Repuestos = x.Repuestos,
                        Cantidad = x.Cantidad,
                        ManoObra = x.ManoObra,
                        Estado = x.Estado,
                        Observaciones = x.Observaciones
                    })
                    .FirstOrDefaultAsync();

                if (orden == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe la orden o fue eliminada.";
                    return NotFound(respuesta);
                }

                respuesta.Ok = 1;
                respuesta.Message = "Orden encontrada";
                respuesta.Data.Add(orden);

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
        public async Task<ActionResult> Create([FromBody] OrdenTrabajoCreateDto dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Cliente))
                    return BadRequest(new { message = "El cliente es requerido." });

                if (string.IsNullOrWhiteSpace(dto.Matricula))
                    return BadRequest(new { message = "La matrícula es requerida." });

                if (string.IsNullOrWhiteSpace(dto.Modelo))
                    return BadRequest(new { message = "El modelo es requerido." });

                if (string.IsNullOrWhiteSpace(dto.Trabajo))
                    return BadRequest(new { message = "El trabajo es requerido." });

                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var orden = new OrdenTrabajo
                {
                    Cliente = dto.Cliente.Trim(),
                    Dni = dto.Dni?.Trim(),
                    Telefono = dto.Telefono?.Trim(),
                    Matricula = dto.Matricula.Trim().ToUpper(),
                    Marca = dto.Marca?.Trim(),
                    Modelo = dto.Modelo.Trim(),
                    Kilometraje = dto.Kilometraje,
                    Fecha = dto.Fecha == default ? DateTime.Now : dto.Fecha,
                    Trabajo = dto.Trabajo.Trim(),
                    ItemsJson = dto.ItemsJson,
                    Repuestos = dto.Repuestos,
                    Cantidad = NormalizeCantidad(dto.Cantidad),
                    ManoObra = dto.ManoObra,
                    Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Recibido" : dto.Estado.Trim(),
                    Observaciones = dto.Observaciones?.Trim(),
                    Eliminado = false
                };

                _context.OrdenesTrabajo.Add(orden);
                _context.Entry(orden).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Orden registrada correctamente.";
                respuesta.Data.Add(new { orden.Id });

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
        public async Task<ActionResult> Update(int id, [FromBody] OrdenTrabajoUpdateDto dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var orden = await _context.OrdenesTrabajo
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (orden == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe la orden o fue eliminada.";
                    return NotFound(respuesta);
                }

                if (IsEditLocked(orden.Estado))
                {
                    return BadRequest(new
                    {
                        message = "No se puede editar una orden en reparacion, lista o entregada."
                    });
                }

                if (!string.IsNullOrWhiteSpace(dto.Cliente)) orden.Cliente = dto.Cliente.Trim();
                if (dto.Dni != null) orden.Dni = dto.Dni.Trim();
                if (dto.Telefono != null) orden.Telefono = dto.Telefono.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Matricula)) orden.Matricula = dto.Matricula.Trim().ToUpper();
                if (dto.Marca != null) orden.Marca = dto.Marca.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Modelo)) orden.Modelo = dto.Modelo.Trim();

                if (dto.Kilometraje.HasValue) orden.Kilometraje = dto.Kilometraje.Value;
                if (dto.Fecha.HasValue) orden.Fecha = dto.Fecha.Value;

                if (!string.IsNullOrWhiteSpace(dto.Trabajo)) orden.Trabajo = dto.Trabajo.Trim();
                orden.ItemsJson = dto.ItemsJson;

                if (dto.Repuestos.HasValue) orden.Repuestos = dto.Repuestos.Value;
                if (dto.Cantidad.HasValue) orden.Cantidad = NormalizeCantidad(dto.Cantidad.Value);
                if (dto.ManoObra.HasValue) orden.ManoObra = dto.ManoObra.Value;

                if (!string.IsNullOrWhiteSpace(dto.Estado)) orden.Estado = dto.Estado.Trim();

                if (dto.Observaciones != null) orden.Observaciones = dto.Observaciones.Trim();

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Orden actualizada correctamente.";
                respuesta.Data.Add(new { orden.Id });

                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpPut("estado/{id:int}")]
        public async Task<ActionResult> UpdateEstado(int id, [FromBody] OrdenTrabajoEstadoDTO dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Estado))
                    return BadRequest(new { message = "El estado es requerido." });

                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var orden = await _context.OrdenesTrabajo
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (orden == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe la orden o fue eliminada.";
                    return NotFound(respuesta);
                }

                orden.Estado = dto.Estado.Trim();

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Estado actualizado correctamente.";
                respuesta.Data.Add(new { orden.Id, orden.Estado });

                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpPut("{id:int}/facturada")]
        public async Task<ActionResult> MarcarFacturada(int id)
        {
            var respuesta = new Respuesta<object>();

            var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
            if (!workshopId.HasValue) return Forbid();

            var orden = await _context.OrdenesTrabajo
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value
                );

            if (orden == null)
            {
                respuesta.Ok = 0;
                respuesta.Message = "No existe la orden.";
                return NotFound(respuesta);
            }

            orden.Facturada = true;

            await _context.SaveChangesAsync();

            respuesta.Ok = 1;
            respuesta.Message = "Orden marcada como facturada.";
            respuesta.Data.Add(new { orden.Id, orden.Facturada });

            return Ok(respuesta);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteSoft(int id)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var orden = await _context.OrdenesTrabajo
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (orden == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe la orden o ya fue eliminada.";
                    return NotFound(respuesta);
                }

                orden.Eliminado = true;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Orden eliminada correctamente.";
                respuesta.Data.Add(new { orden.Id, orden.Eliminado });

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

