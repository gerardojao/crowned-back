using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FacturaEmitidaController : ControllerBase
    {
        private readonly dbContext _context;

        public FacturaEmitidaController(dbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] FacturaEmitida dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.NumeroFactura))
                    return BadRequest(new { message = "El número de factura es requerido." });

                if (string.IsNullOrWhiteSpace(dto.Cliente))
                    return BadRequest(new { message = "El cliente es requerido." });

                var existe = await _context.FacturasEmitidas
                    .AnyAsync(x => x.NumeroFactura == dto.NumeroFactura && !x.Eliminado);

                if (existe)
                    return BadRequest(new { message = "Ya existe una factura emitida con ese número." });

                var factura = new FacturaEmitida
                {
                    NumeroFactura = dto.NumeroFactura.Trim(),
                    IdOrdenTrabajo = dto.IdOrdenTrabajo,
                    Fecha = dto.Fecha == default ? DateTime.UtcNow : dto.Fecha,

                    Cliente = dto.Cliente.Trim(),
                    Dni = dto.Dni?.Trim(),
                    DireccionCliente = dto.DireccionCliente?.Trim(),
                    TelefonoCliente = dto.TelefonoCliente?.Trim(),
                    Matricula = dto.Matricula?.Trim().ToUpper(),
                    Km = dto.Km?.Trim(),

                    Subtotal = dto.Subtotal,
                    Iva = dto.Iva,
                    Otros = dto.Otros,
                    Total = dto.Total,

                    Observaciones = dto.Observaciones?.Trim(),
                    ItemsJson = dto.ItemsJson,

                    Eliminado = false
                };

                _context.FacturasEmitidas.Add(factura);

                await CrearIngresoAutomaticoSiAplica(factura);

                if (factura.IdOrdenTrabajo.HasValue)
                {
                    var orden = await _context.OrdenesTrabajo
                        .FirstOrDefaultAsync(x => x.Id == factura.IdOrdenTrabajo.Value && !x.Eliminado);

                    if (orden != null)
                    {
                        orden.Facturada = true;
                    }
                }

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Factura emitida guardada correctamente.";
                respuesta.Data.Add(new
                {
                    factura.Id,
                    factura.NumeroFactura,
                    factura.IdOrdenTrabajo
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

        private async Task CrearIngresoAutomaticoSiAplica(FacturaEmitida factura)
        {
            if (string.IsNullOrWhiteSpace(factura.ItemsJson))
                return;

            var items = JsonSerializer.Deserialize<List<FacturaItemDTO>>(
                factura.ItemsJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (items == null || items.Count == 0)
                return;

            var reglas = new Dictionary<string, Func<string, bool>>
            {
                ["Servicio cambio de aceite y filtro"] = d =>
                    d.Contains("cambio") &&
                    d.Contains("aceite") &&
                    d.Contains("filtro"),

                ["Mano de obra"] = d =>
                    d.Contains("mano") &&
                    d.Contains("obra"),

                ["Repuestos"] = d =>
                    d.Contains("repuesto"),

                ["Servicio a terceros"] = d =>
                    d.Contains("tercero") ||
                    d.Contains("servicio externo")
            };

            foreach (var item in items)
            {
                var descripcion = item.Descripcion?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(descripcion))
                    continue;

                var descLower = descripcion.ToLower();

                foreach (var regla in reglas)
                {
                    if (!regla.Value(descLower))
                        continue;

                    var nombreCuenta = regla.Key;

                    var cuentaIngreso = await _context.Ingresos
                        .FirstOrDefaultAsync(x =>
                            x.NombreIngreso.ToLower() == nombreCuenta.ToLower()
                        );

                    if (cuentaIngreso == null)
                    {
                        cuentaIngreso = new Ingreso
                        {
                            NombreIngreso = nombreCuenta
                        };

                        _context.Ingresos.Add(cuentaIngreso);
                        await _context.SaveChangesAsync();
                    }

                    var importe = item.Cantidad * item.Importe;

                    var yaExiste = await _context.FichaIngresos.AnyAsync(x =>
                        !x.Eliminado &&
                        x.NombreIngreso == cuentaIngreso.Id &&
                        x.Descripcion != null &&
                        x.Descripcion.Contains(factura.NumeroFactura)
                    );

                    if (yaExiste)
                        break;

                    _context.FichaIngresos.Add(new FichaIngreso
                    {
                        NombreIngreso = cuentaIngreso.Id,
                        Fecha = factura.Fecha,
                        Mes = factura.Fecha.ToString("MMMM", new CultureInfo("es-ES")),
                        Descripcion = $"Factura {factura.NumeroFactura} - {factura.Cliente} - {descripcion}",
                        Importe = importe,
                        Eliminado = false,
                        FechaEliminacion = null
                    });

                    break;
                }
            }
        }

        [HttpGet("numero/{numeroFactura}")]
        public async Task<ActionResult> GetByNumero(string numeroFactura)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var factura = await _context.FacturasEmitidas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.NumeroFactura == numeroFactura &&
                        !x.Eliminado
                    );

                if (factura == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe una factura con ese número.";
                    return NotFound(respuesta);
                }

                respuesta.Ok = 1;
                respuesta.Message = "Factura encontrada.";
                respuesta.Data.Add(factura);

                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpGet("orden/{idOrden:int}")]
        public async Task<ActionResult> GetByOrden(int idOrden)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var factura = await _context.FacturasEmitidas
                    .AsNoTracking()
                    .Where(x =>
                        x.IdOrdenTrabajo == idOrden &&
                        !x.Eliminado
                    )
                    .OrderByDescending(x => x.Fecha)
                    .FirstOrDefaultAsync();

                if (factura == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe factura emitida para esta orden.";
                    return NotFound(respuesta);
                }

                respuesta.Ok = 1;
                respuesta.Message = "Factura encontrada.";
                respuesta.Data.Add(factura);

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
    public class FacturaItemDTO
    {
        public string? Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Importe { get; set; }
    }
}