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

        public PresupuestoController(dbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(
     [FromQuery] string? matricula,
     [FromQuery] string? cliente,
     [FromQuery] string? estado,
     [FromQuery] DateTime? fechaDesde,
     [FromQuery] DateTime? fechaHasta)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var query = _context.Presupuestos
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
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

                var data = await query
                    .OrderByDescending(x => x.Fecha)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Presupuestos";
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

        [HttpGet("ultimos")]
        public async Task<ActionResult> GetUltimos([FromQuery] int take = 10)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                if (take <= 0) take = 10;
                if (take > 100) take = 100;

                var data = await _context.Presupuestos
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
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
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var presupuesto = await _context.Presupuestos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
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

                var anio = DateTime.Now.Year;

                var ultimoNumero = await _context.Presupuestos
                    .Where(x => x.Fecha.Year == anio)
                    .CountAsync();

                var numero = string.IsNullOrWhiteSpace(dto.NumeroPresupuesto)
                    ? $"P-{ultimoNumero + 1}-{anio}"
                    : dto.NumeroPresupuesto.Trim();

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
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
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

                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(x => x.Id == id && !x.Eliminado);

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
                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(x => x.Id == id && !x.Eliminado);

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
                var presupuesto = await _context.Presupuestos
                    .FirstOrDefaultAsync(x => x.Id == id && !x.Eliminado);

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