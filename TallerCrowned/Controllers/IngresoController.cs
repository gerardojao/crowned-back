using FamilyApp.Data;
using FamilyApp.DTOs.Ingresos;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FamilyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngresoController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly IWebHostEnvironment _env;
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public IngresoController(IRepository repository, IWebHostEnvironment env, dbContext context, ICurrentUserService currentUserService)
        {
            _repository = repository;
            _env = env;
            _context = context;
            _currentUserService = currentUserService;
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Ingreso>>> GetIngresos()
        //{
        //    Respuesta<object> respuesta = new();
        //    try
        //    {
        //        var ingresos = await _repository.SelectAll<Ingreso>();
        //        if (ingresos != null)
        //        {
        //            foreach (var item in ingresos)
        //            {
        //                respuesta.Data.Add(new
        //                {
        //                    item.Id,
        //                    item.NombreIngreso
        //                });
        //            }
        //            respuesta.Ok = 1;
        //            respuesta.Message = "Ingresos Registrados";
        //        }
        //        else
        //        {
        //            respuesta.Ok = 0;
        //            respuesta.Message = "No se encuenta ningun ingreso";
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        respuesta.Ok = 0;
        //        respuesta.Message = e.Message + " " + e.InnerException;
        //    }
        //    return Ok(respuesta);

        //}
       

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ingreso>>> GetIngresos()
        {
            Respuesta<object> respuesta = new();
            try
            {
                var egresos = await _repository.SelectAll<Ingreso>();
                if (egresos != null)
                {
                    foreach (var item in egresos)
                    {
                        respuesta.Data.Add(new
                        {
                            item.Id,
                            item.NombreIngreso
                        });
                    }
                    respuesta.Ok = 1;
                    respuesta.Message = "Egresos cargados en sistema";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No se encuenta ningun egreso";
                }
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);

        }


        [Authorize]
        [HttpGet("detalle")]
        public async Task<ActionResult> GetIngresosDetalle(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] int? tipoId)
        {
            var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
           // if (uid == 0) return Unauthorized();

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin < fechaInicio)
                return BadRequest(new { message = "La fecha fin no puede ser menor que la fecha inicio." });

            var start = fechaInicio?.Date;
            var endExcl = fechaFin?.Date.AddDays(1);
            var isAdmin = User.IsInRole("admin");
            var raw = await _context.FichaIngresos
                .AsNoTracking()                
                .Where(f => !f.Eliminado
                    && (isAdmin || EF.Property<string>(f, "UsuarioCreacion") == uidStr)
                    && (!start.HasValue || f.Fecha >= start)
                    && (!endExcl.HasValue || f.Fecha < endExcl)
                    && (!tipoId.HasValue || f.NombreIngreso == tipoId.Value))
                .Join(_context.Ingresos.AsNoTracking(),
                      f => f.NombreIngreso, i => i.Id,
                      (f, i) => new {
                          Id = f.Id,
                          Fecha = f.Fecha,
                          Mes = f.Mes,
                          TipoId = i.Id,
                          Tipo = i.NombreIngreso,
                          Descripcion = f.Descripcion,
                          Importe = f.Importe
                      })
                .OrderByDescending(x => x.Fecha ?? DateTime.MinValue)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
            Console.WriteLine(raw);
            var respuesta = new Respuesta<object>
            {
                Ok = 1,
                Message = "Detalle de ingresos",
                Data = { raw }
            };
            return Ok(respuesta);
        }



        //GET: api/SolicitudIngreso
        [HttpGet("totales")]
        public async Task<ActionResult<IEnumerable<Ingreso>>> GetAllIngresos()
        {
            Respuesta<object> respuesta = new();
            try
            {
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var ingre = await (from _ingreso in _context.Ingresos
                                   join _fIngreso in _context.FichaIngresos on _ingreso.Id equals _fIngreso.NombreIngreso
                                   where !_fIngreso.Eliminado &&
                                   (isAdmin || EF.Property<string>(_fIngreso, "UsuarioCreacion") == uidStr)                          // <- filtro
                                   select new { _ingreso.NombreIngreso, _fIngreso.Importe })
                                  .OrderByDescending(x => x.Importe)
                                  .ToListAsync();

                if (ingre != null)
                {
                    var ingreT = from i in ingre
                                 group i by i.NombreIngreso into totals
                                 select new
                                 {
                                     Cuenta_Ingreso = totals.Key,
                                     Total = totals.Sum(e => e.Importe)
                                 };
                    respuesta.Data.Add(ingreT);
                    respuesta.Ok = 1;
                    respuesta.Message = "Ingresos e Importe";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No hay Ingresos";
                }
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);
        }

       
        [HttpGet("totalesPorMes")]
        public async Task<ActionResult> GetIngresosPorMesQuery([FromQuery] DateTime fechaInicio,
                                                         [FromQuery] DateTime fechaFin)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                var fi = fechaInicio.Date;
                var ffExcl = fechaFin.Date.AddDays(1); // [fi, ffExcl)

                var ingre = await (from i in _context.Ingresos.AsNoTracking()
                                   join f in _context.FichaIngresos.AsNoTracking() on i.Id equals f.NombreIngreso
                                   where !f.Eliminado                                  // <- filtro
                                      && f.Fecha.HasValue
                                      && f.Fecha.Value >= fi
                                      && f.Fecha.Value < ffExcl
                                   select new { i.NombreIngreso, f.Importe })
                                  .OrderByDescending(x => x.Importe)
                                  .ToListAsync();

                var ingreT = ingre.GroupBy(x => x.NombreIngreso)
                                  .Select(g => new { Cuenta_Ingreso = g.Key, Total = g.Sum(x => x.Importe) });

                respuesta.Data.Add(ingreT);
                respuesta.Ok = 1;
                respuesta.Message = "Ingresos e Importe";
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Ok = 0; respuesta.Message = ex.Message;
                return Ok(respuesta);
            }
        }


        // POST: api/Egreso
        [HttpPost]
        public async Task<ActionResult> PostIngreso(Ingreso ingreso)
        {

            Respuesta<object> respuesta = new();
            try
            {

                await _repository.CreateAsync(ingreso);
                respuesta.Ok = 1;
                respuesta.Message = "Ingreso registrado satisfactoriamente";


            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
                return Ok(respuesta);
            }
            return Ok(respuesta);
        }

        // PUT parcial: actualizar detalle
        [HttpPut("detalle/{id:int}")]
        public async Task<ActionResult> PutDetalle(int id, [FromBody] FichaIngresoUpdateDTO dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var fi = await _context.FichaIngresos.FirstOrDefaultAsync(x => x.Id == id && !x.Eliminado);
                if (fi == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el detalle o fue eliminado.";
                    return NotFound(respuesta);
                }

                if (dto.Fecha.HasValue) fi.Fecha = dto.Fecha;
                if (!string.IsNullOrWhiteSpace(dto.Mes)) fi.Mes = dto.Mes;
                if (dto.NombreIngreso.HasValue) fi.NombreIngreso = dto.NombreIngreso.Value;
                if (dto.Descripcion != null) fi.Descripcion = dto.Descripcion; // permite null para limpiar
                if (dto.Importe.HasValue) fi.Importe = dto.Importe.Value;
                if (dto.Foto != null) fi.Foto = dto.Foto;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Detalle actualizado correctamente.";
                respuesta.Data.Add(new { fi.Id });
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        // DELETE lógico: marcar como eliminado
        [HttpDelete("detalle/{id:int}")]
        public async Task<ActionResult> DeleteDetalleSoft(int id)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                var fi = await _context.FichaIngresos.FirstOrDefaultAsync(x => x.Id == id && !x.Eliminado);
                if (fi == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el detalle o ya está eliminado.";
                    return NotFound(respuesta);
                }

                fi.Eliminado = true;
                fi.FechaEliminacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Detalle eliminado (lógico) correctamente.";
                respuesta.Data.Add(new { fi.Id, fi.Eliminado, fi.FechaEliminacion });
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
        public async Task<ActionResult> GetUltimos([FromQuery] int take = 5)
        {
            var respuesta = new Respuesta<object>();
            var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
            var isAdmin = User.IsInRole("admin");
            try
            {
                if (take <= 0) take = 5;
                if (take > 100) take = 100; // cota de seguridad

                // Ingresos (filtrando soft delete si lo tienes en la tabla)
                var ingresosQ =
                    from f in _context.FichaIngresos.AsNoTracking()
                    join i in _context.Ingresos.AsNoTracking() on f.NombreIngreso equals i.Id
                    where !f.Eliminado &&
                                   (isAdmin || EF.Property<string>(f, "UsuarioCreacion") == uidStr)
                    select new MovimientoDTO
                    {
                        Id = f.Id,
                        Fecha = f.Fecha,
                        Mes = f.Mes,
                        TipoId = i.Id,
                        Tipo = i.NombreIngreso,
                        Descripcion = f.Descripcion,
                        Importe = f.Importe,
                        Kind = "ingreso"
                    };

                // Egresos
                var egresosQ =
                    from f in _context.FichaEgresos.AsNoTracking()
                    join e in _context.Egresos.AsNoTracking() on f.NombreEgreso equals e.Id
                    where !f.Eliminado  &&
                                   (isAdmin || EF.Property<string>(f, "UsuarioCreacion") == uidStr)
                    select new MovimientoDTO
                    {
                        Id = f.Id,
                        Fecha = f.Fecha,
                        Mes = f.Mes,
                        TipoId = e.Id,
                        Tipo = e.Nombre,
                        Descripcion = f.Descripcion,
                        Importe = f.Importe,
                        Kind = "egreso"
                    };

                // Materializamos por separado y combinamos en memoria
                var ingresos = await ingresosQ.ToListAsync();
                var egresos = await egresosQ.ToListAsync();

                var ultimos = ingresos
                    .Concat(egresos)
                    .OrderByDescending(x => x.Fecha ?? DateTime.MinValue)
                    .ThenByDescending(x => x.Id)
                    .Take(take)
                    .ToList();

                respuesta.Ok = 1;
                respuesta.Message = "Últimos movimientos";
                respuesta.Data.Add(ultimos);
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Ok = 0;
                respuesta.Message = ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

    }
}
