using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FamilyApp.Data;
using FamilyApp.Models;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NumeradorFacturaController : ControllerBase
    {
        private readonly dbContext _context;

        public NumeradorFacturaController(dbContext context)
        {
            _context = context;
        }

        [HttpPost("siguiente")]
        public async Task<ActionResult> Siguiente()
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var anio = DateTime.Now.Year;

                using var transaction = await _context.Database.BeginTransactionAsync();

                var numerador = await _context.NumeradoresFactura
                    .FirstOrDefaultAsync(x => x.Anio == anio);

                if (numerador == null)
                {
                    numerador = new NumeradorFactura
                    {
                        Anio = anio,
                        UltimoNumero = 0
                    };

                    _context.NumeradoresFactura.Add(numerador);
                    await _context.SaveChangesAsync();
                }

                numerador.UltimoNumero += 1;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Número de factura generado.";
                respuesta.Data.Add(new
                {
                    numero = numerador.UltimoNumero,
                    anio,
                    numeroFactura = $"{numerador.UltimoNumero}-{anio}"
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

        [HttpGet("preview")]
        public async Task<ActionResult> Preview()
        {
            var respuesta = new Respuesta<object>();

            var anio = DateTime.Now.Year;

            var ultimoNumero = await _context.NumeradoresFactura
                .Where(x => x.Anio == anio)
                .Select(x => (int?)x.UltimoNumero)
                .FirstOrDefaultAsync() ?? 0;

            var siguiente = ultimoNumero + 1;

            respuesta.Ok = 1;
            respuesta.Message = "Próximo número de factura.";
            respuesta.Data.Add(new
            {
                numero = siguiente,
                anio,
                numeroFactura = $"{siguiente}-{anio}"
            });

            return Ok(respuesta);
        }
    }
}