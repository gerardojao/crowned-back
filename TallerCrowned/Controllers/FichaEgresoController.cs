

using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Mvc;
using FamilyApp.DTOs.Egresos;
using Microsoft.EntityFrameworkCore;

namespace FamilyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FichaEgresoController : ControllerBase
    {

        private readonly IRepository _repository;
        private readonly IWebHostEnvironment _env;


        public FichaEgresoController(IRepository repository, IWebHostEnvironment env)
        {
           _repository = repository;
            _env = env;
     
        }

        //// GET: api/SolicitudFichaEgresos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FichaEgreso>>> GetAllEgresos()
        {
            Respuesta<object> respuesta = new();
            try
            {
                var fEgresos = await _repository.SelectAll<FichaEgreso>();
                if (fEgresos != null)
                {
                    foreach (var item in fEgresos)
                    {
                        respuesta.Data.Add(item);
                    }
                    respuesta.Ok = 1;
                    respuesta.Message = "Egresos registrado";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No hay egresos registrados";
                }

            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);
        }


        // POST: api/FichaEgreso
        [HttpPost]
        public async Task<ActionResult> PostFichaEgreso([FromBody] FichaEgreso fichaEgreso)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                // saneo básico (si lo necesitas)
                fichaEgreso.Eliminado = false;
                fichaEgreso.FechaEliminacion = null;

                var newId = await _repository.CreateAsync(fichaEgreso);

                respuesta.Ok = 1;
                respuesta.Message = "Success";
                respuesta.Data.Add(new { Id = newId });
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        // PUT parcial: api/FichaEgreso/detalle/5
        [HttpPut("detalle/{id:int}")]
        public async Task<ActionResult> PutDetalleEgreso(int id, [FromBody] FichaEgresoUpdateDTO dto)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                var fe = await _repository.SelectById<FichaEgreso>(id);
                if (fe == null || fe.Eliminado)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el detalle o fue eliminado.";
                    return NotFound(respuesta);
                }

                // Patch: solo asigna lo que venga en el DTO
                if (dto.Fecha.HasValue) fe.Fecha = dto.Fecha;
                if (!string.IsNullOrWhiteSpace(dto.Mes)) fe.Mes = dto.Mes;
                if (dto.NombreEgreso.HasValue) fe.NombreEgreso = dto.NombreEgreso.Value;
                if (dto.Descripcion != null) fe.Descripcion = dto.Descripcion; // acepta null
                if (dto.Importe.HasValue) fe.Importe = dto.Importe.Value;
                if (dto.Foto != null) fe.Foto = dto.Foto;

                await _repository.UpdateAsync(fe);

                respuesta.Ok = 1;
                respuesta.Message = "Detalle de egreso actualizado correctamente.";
                respuesta.Data.Add(new { fe.Id });
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        // DELETE lógico: api/FichaEgreso/detalle/5
        [HttpDelete("detalle/{id:int}")]
        public async Task<ActionResult> DeleteDetalleEgresoSoft(int id)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                var fe = await _repository.SelectById<FichaEgreso>(id);
                if (fe == null || fe.Eliminado)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el detalle o ya está eliminado.";
                    return NotFound(respuesta);
                }

                fe.Eliminado = true;
                fe.FechaEliminacion = DateTime.UtcNow;

                await _repository.UpdateAsync(fe);

                respuesta.Ok = 1;
                respuesta.Message = "Detalle de egreso eliminado (lógico) correctamente.";
                respuesta.Data.Add(new { fe.Id, fe.Eliminado, fe.FechaEliminacion });
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
