using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FamilyApp.Data;
using FamilyApp.Models;
using TallerCrowned.Models;
using System.Data;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NumeradorFacturaController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public NumeradorFacturaController(
            dbContext context,
            ICurrentUserService currentUserService,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpPost("siguiente")]
        public async Task<ActionResult> Siguiente([FromQuery] string serie = "A")
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var anio = DateTime.Now.Year;
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();
                var ownerKey = GetOwnerKey();
                serie = NormalizeSerie(serie);

                using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                var numerador = await _context.NumeradoresFactura
                    .FirstOrDefaultAsync(x =>
                        x.WorkshopId == workshopId.Value &&
                        x.Serie == serie &&
                        x.Anio == anio
                    );

                if (numerador == null)
                {
                    numerador = new NumeradorFactura
                    {
                        WorkshopId = workshopId.Value,
                        OwnerKey = ownerKey,
                        Serie = serie,
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
                    serie,
                    numeroFactura = FormatNumeroFactura(serie, workshopId.Value, numerador.UltimoNumero, anio)
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
        public async Task<ActionResult> Preview([FromQuery] string serie = "A")
        {
            var respuesta = new Respuesta<object>();

            var anio = DateTime.Now.Year;
            var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
            if (!workshopId.HasValue) return Forbid();
            var ownerKey = GetOwnerKey();
            serie = NormalizeSerie(serie);

            var ultimoNumero = await _context.NumeradoresFactura
                .Where(x =>
                    x.WorkshopId == workshopId.Value &&
                    x.Serie == serie &&
                    x.Anio == anio
                )
                .Select(x => (int?)x.UltimoNumero)
                .FirstOrDefaultAsync() ?? 0;

            var siguiente = ultimoNumero + 1;

            respuesta.Ok = 1;
            respuesta.Message = "Próximo número de factura.";
            respuesta.Data.Add(new
            {
                numero = siguiente,
                anio,
                serie,
                numeroFactura = FormatNumeroFactura(serie, workshopId.Value, siguiente, anio)
            });

            return Ok(respuesta);
        }

        private string GetOwnerKey()
        {
            return _currentUserService.UserIdInt?.ToString()
                ?? _currentUserService.UserIdOrEmail
                ?? "system";
        }

        private static string NormalizeSerie(string? serie)
        {
            var clean = string.IsNullOrWhiteSpace(serie) ? "A" : serie.Trim().ToUpperInvariant();
            return clean.Length > 20 ? clean[..20] : clean;
        }

        private static string FormatNumeroFactura(string serie, int workshopId, int numero, int anio)
        {
            return $"{serie}-{anio}-T{workshopId}-{numero:D4}";
        }
    }
}
