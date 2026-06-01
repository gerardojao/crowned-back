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
    public class PresupuestoController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public PresupuestoController(
            dbContext context,
            ICurrentUserService currentUserService,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(
     [FromQuery] string? matricula,
     [FromQuery] string? cliente,
     [FromQuery] string? estado,
     [FromQuery] DateTime? fechaDesde,
     [FromQuery] DateTime? fechaHasta,
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var query = _context.Presupuestos
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (!string.IsNullOrWhiteSpace(matricula))
                {
                    var m = matricula.Trim().ToLower();
                    query = query.Where(x => x.Matricula.ToLower().Contains(m));
                }

                if (!string.IsNullOrWhiteSpace(cliente))
                {
                    var c = cliente.Trim().ToLower();
                    query = query.Where(x => x.Cliente.ToLower().Contains(c));
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    var e = estado.Trim().ToLower();
                    query = query.Where(x => x.Estado.ToLower() == e);
                }

                if (fechaDesde.HasValue)
                {
                    var desde = fechaDesde.Value.Date;
                    query = query.Where(x => x.Fecha >= desde);
                }

                if (fechaHasta.HasValue)
                {
                    var hasta = fechaHasta.Value.Date.AddDays(1);
                    query = query.Where(x => x.Fecha < hasta);
                }

                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var total = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.Fecha)
                    .ThenByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Presupuestos";
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

        [HttpGet("ultimos")]
        public async Task<ActionResult> GetUltimos([FromQuery] int take = 10)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                if (take <= 0) take = 10;
                if (take > 100) take = 100;

                var data = await _context.Presupuestos
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    )
                    .OrderByDescending(x => x.Fecha)
                    .ThenByDescending(x => x.Id)
                    .Take(take)
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Últimos presupuestos";
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

                var presupuesto = await _context.Presupuestos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (presupuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el presupuesto.";
                    return NotFound(respuesta);
                }

                respuesta.Ok = 1;
                respuesta.Message = "Presupuesto encontrado.";
                respuesta.Data.Add(presupuesto);

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
        public async Task<ActionResult> Create([FromBody] Presupuesto dto)
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

                var anio = DateTime.Now.Year;
                var workshop = await _context.Workshops
                    .AsNoTracking()
                    .Where(x => x.Id == workshopId.Value)
                    .Select(x => new { x.SerieFactura })
                    .FirstOrDefaultAsync();

                var ultimoNumero = await _context.Presupuestos
                    .Where(x => x.Fecha.Year == anio && EF.Property<int>(x, "WorkshopId") == workshopId.Value)
                    .CountAsync();

                var serie = workshop?.SerieFactura?.Trim().ToUpperInvariant();
                string BuildNumero(int sequential) =>
                    string.IsNullOrWhiteSpace(serie) || serie == "A"
                        ? $"P-{sequential}-{anio}"
                        : $"{serie}-P-{anio}-{sequential:0000}";

                var numero = string.IsNullOrWhiteSpace(dto.NumeroPresupuesto)
                    ? BuildNumero(ultimoNumero + 1)
                    : dto.NumeroPresupuesto.Trim();

                if (string.IsNullOrWhiteSpace(dto.NumeroPresupuesto))
                {
                    var next = ultimoNumero + 1;
                    while (await _context.Presupuestos.AnyAsync(x =>
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value &&
                        x.NumeroPresupuesto == numero))
                    {
                        next++;
                        numero = BuildNumero(next);
                    }
                }

                var presupuesto = new Presupuesto
                {
                    NumeroPresupuesto = numero,
                    Cliente = dto.Cliente.Trim(),
                    Telefono = dto.Telefono?.Trim(),
                    Matricula = dto.Matricula.Trim().ToUpper(),
                    Marca = dto.Marca?.Trim(),
                    Modelo = dto.Modelo.Trim(),
                    Kilometraje = dto.Kilometraje,
                    Fecha = dto.Fecha == default ? DateTime.Now : dto.Fecha,
                    Trabajo = dto.Trabajo.Trim(),
                    Repuestos = dto.Repuestos,
                    ManoObra = dto.ManoObra,
                    Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Pendiente" : dto.Estado.Trim(),
                    Observaciones = dto.Observaciones?.Trim(),
                    ConvertidoEnOrden = false,
                    IdOrdenTrabajo = null,
                    Eliminado = false
                };

                _context.Presupuestos.Add(presupuesto);
                _context.Entry(presupuesto).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Presupuesto registrado correctamente.";
                respuesta.Data.Add(new
                {
                    presupuesto.Id,
                    presupuesto.NumeroPresupuesto
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

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] Presupuesto dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (presupuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el presupuesto.";
                    return NotFound(respuesta);
                }

                if (!string.IsNullOrWhiteSpace(dto.Cliente)) presupuesto.Cliente = dto.Cliente.Trim();
                if (dto.Telefono != null) presupuesto.Telefono = dto.Telefono.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Matricula)) presupuesto.Matricula = dto.Matricula.Trim().ToUpper();
                if (dto.Marca != null) presupuesto.Marca = dto.Marca.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Modelo)) presupuesto.Modelo = dto.Modelo.Trim();

                if (dto.Kilometraje.HasValue) presupuesto.Kilometraje = dto.Kilometraje.Value;
                if (dto.Fecha != default) presupuesto.Fecha = dto.Fecha;

                if (!string.IsNullOrWhiteSpace(dto.Trabajo)) presupuesto.Trabajo = dto.Trabajo.Trim();

                presupuesto.Repuestos = dto.Repuestos;
                presupuesto.ManoObra = dto.ManoObra;

                if (!string.IsNullOrWhiteSpace(dto.Estado)) presupuesto.Estado = dto.Estado.Trim();

                if (dto.Observaciones != null) presupuesto.Observaciones = dto.Observaciones.Trim();

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Presupuesto actualizado correctamente.";
                respuesta.Data.Add(new { presupuesto.Id });

                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpPut("{id:int}/estado")]
        public async Task<ActionResult> UpdateEstado(int id, [FromBody] Presupuesto dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Estado))
                    return BadRequest(new { message = "El estado es requerido." });

                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (presupuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el presupuesto.";
                    return NotFound(respuesta);
                }

                presupuesto.Estado = dto.Estado.Trim();

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Estado actualizado correctamente.";
                respuesta.Data.Add(new
                {
                    presupuesto.Id,
                    presupuesto.Estado
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

        [HttpPost("{id:int}/convertir-orden")]
        public async Task<ActionResult> ConvertirEnOrden(int id)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (presupuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el presupuesto.";
                    return NotFound(respuesta);
                }

                if (presupuesto.ConvertidoEnOrden)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "Este presupuesto ya fue convertido en orden.";
                    return BadRequest(respuesta);
                }

                var orden = new OrdenTrabajo
                {
                    Cliente = presupuesto.Cliente,
                    Telefono = presupuesto.Telefono,
                    Matricula = presupuesto.Matricula,
                    Marca = presupuesto.Marca,
                    Modelo = presupuesto.Modelo,
                    Kilometraje = presupuesto.Kilometraje,
                    Fecha = DateTime.Now,
                    Trabajo = presupuesto.Trabajo,
                    Repuestos = presupuesto.Repuestos,
                    ManoObra = presupuesto.ManoObra,
                    Estado = "Recibido",
                    Observaciones = presupuesto.Observaciones,
                    Eliminado = false
                };

                _context.OrdenesTrabajo.Add(orden);
                _context.Entry(orden).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                presupuesto.ConvertidoEnOrden = true;
                presupuesto.IdOrdenTrabajo = orden.Id;
                presupuesto.Estado = "Convertido";

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Presupuesto convertido en orden correctamente.";
                respuesta.Data.Add(new
                {
                    presupuesto.Id,
                    presupuesto.IdOrdenTrabajo
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

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteSoft(int id)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (presupuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el presupuesto o ya fue eliminado.";
                    return NotFound(respuesta);
                }

                presupuesto.Eliminado = true;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Presupuesto eliminado correctamente.";
                respuesta.Data.Add(new
                {
                    presupuesto.Id,
                    presupuesto.Eliminado
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


