using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FacturaEmitidaController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public FacturaEmitidaController(dbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] FacturaEmitida dto)
        {
            var items = DeserializeItems(dto.ItemsJson);
            var request = new FacturaEmitidaCreateDto
            {
                NumeroFactura = dto.NumeroFactura,
                IdOrdenTrabajo = dto.IdOrdenTrabajo,
                Fecha = dto.Fecha == default ? null : dto.Fecha,
                Cliente = dto.Cliente,
                Dni = dto.Dni,
                DireccionCliente = dto.DireccionCliente,
                TelefonoCliente = dto.TelefonoCliente,
                Matricula = dto.Matricula,
                Km = dto.Km,
                Observaciones = dto.Observaciones,
                Otros = dto.Otros,
                Items = items,
                ItemsJson = dto.ItemsJson
            };

            return await EmitirInterno(request, allowProvidedNumber: true);
        }

        [HttpPost("emitir")]
        public async Task<ActionResult> Emitir([FromBody] FacturaEmitidaCreateDto dto)
        {
            return await EmitirInterno(dto, allowProvidedNumber: false);
        }

        private async Task<ActionResult> EmitirInterno(FacturaEmitidaCreateDto dto, bool allowProvidedNumber)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Cliente))
                    return BadRequest(new { message = "El cliente es requerido." });

                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");
                var ownerKey = GetOwnerKey();
                var serie = NormalizeSerie(dto.Serie);
                var anio = DateTime.Now.Year;
                var fecha = dto.Fecha ?? DateTime.UtcNow;
                var items = NormalizeItems(dto.Items, dto.ItemsJson);

                if (items.Count == 0)
                    return BadRequest(new { message = "La factura debe tener al menos una linea." });

                if (dto.IdOrdenTrabajo.HasValue)
                {
                    var ordenExiste = await _context.OrdenesTrabajo.AnyAsync(x =>
                        x.Id == dto.IdOrdenTrabajo.Value &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                    if (!ordenExiste)
                        return NotFound(new { message = "No existe la orden o no pertenece al usuario actual." });
                }

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                var numeroFactura = allowProvidedNumber && !string.IsNullOrWhiteSpace(dto.NumeroFactura)
                    ? dto.NumeroFactura.Trim()
                    : await GenerateNumeroFactura(ownerKey, serie, anio);

                var existe = await _context.FacturasEmitidas.AnyAsync(x =>
                    x.NumeroFactura == numeroFactura &&
                    !x.Eliminado &&
                    (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                );

                if (existe)
                    return BadRequest(new { message = "Ya existe una factura emitida con ese numero." });

                var subtotal = Round2(items.Sum(x => x.Cantidad * x.Importe));
                var ivaPct = dto.IvaPct <= 0 ? 21 : dto.IvaPct;
                var iva = Round2(subtotal * (ivaPct / 100m));
                var otros = Round2(dto.Otros);
                var total = Round2(subtotal + iva - otros);
                var itemsJson = JsonSerializer.Serialize(
                    items,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                var factura = new FacturaEmitida
                {
                    NumeroFactura = numeroFactura,
                    IdOrdenTrabajo = dto.IdOrdenTrabajo,
                    Fecha = fecha,
                    Cliente = dto.Cliente.Trim(),
                    Dni = dto.Dni?.Trim(),
                    DireccionCliente = dto.DireccionCliente?.Trim(),
                    TelefonoCliente = dto.TelefonoCliente?.Trim(),
                    Matricula = dto.Matricula?.Trim().ToUpperInvariant(),
                    Km = dto.Km?.Trim(),
                    Subtotal = subtotal,
                    Iva = iva,
                    Otros = otros,
                    Total = total,
                    Observaciones = dto.Observaciones?.Trim(),
                    ItemsJson = itemsJson,
                    Eliminado = false
                };

                _context.FacturasEmitidas.Add(factura);
                await _context.SaveChangesAsync();

                await CrearIngresoAutomaticoSiAplica(factura, items);

                if (factura.IdOrdenTrabajo.HasValue)
                {
                    var orden = await _context.OrdenesTrabajo.FirstOrDefaultAsync(x =>
                        x.Id == factura.IdOrdenTrabajo.Value &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                    if (orden != null)
                        orden.Facturada = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Factura emitida guardada correctamente.";
                respuesta.Data.Add(new
                {
                    factura.Id,
                    factura.NumeroFactura,
                    factura.IdOrdenTrabajo,
                    factura.Subtotal,
                    factura.Iva,
                    factura.Otros,
                    factura.Total,
                    IvaPct = ivaPct
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

        private async Task<string> GenerateNumeroFactura(string ownerKey, string serie, int anio)
        {
            var numerador = await _context.NumeradoresFactura.FirstOrDefaultAsync(x =>
                x.OwnerKey == ownerKey &&
                x.Serie == serie &&
                x.Anio == anio
            );

            if (numerador == null)
            {
                numerador = new NumeradorFactura
                {
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

            return FormatNumeroFactura(serie, ownerKey, numerador.UltimoNumero, anio);
        }

        private async Task CrearIngresoAutomaticoSiAplica(FacturaEmitida factura, List<FacturaItemDTO> items)
        {
            if (items.Count == 0)
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

            var debeCrearAlertaAceite = false;

            foreach (var item in items)
            {
                var descripcionOriginal = item.Descripcion?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(descripcionOriginal))
                    continue;

                string descLower = NormalizarTexto(descripcionOriginal);

                foreach (var regla in reglas)
                {
                    if (!regla.Value(descLower))
                        continue;

                    var nombreCuenta = regla.Key;

                    if (nombreCuenta == "Servicio cambio de aceite y filtro")
                        debeCrearAlertaAceite = true;

                    var cuentaIngreso = await _context.Ingresos
                        .FirstOrDefaultAsync(x => x.NombreIngreso.ToLower() == nombreCuenta.ToLower());

                    if (cuentaIngreso == null)
                    {
                        cuentaIngreso = new Ingreso
                        {
                            NombreIngreso = nombreCuenta
                        };

                        _context.Ingresos.Add(cuentaIngreso);
                        await _context.SaveChangesAsync();
                    }

                    var importe = Round2(item.Cantidad * item.Importe);

                    var yaExisteIngreso = await _context.FichaIngresos.AnyAsync(x =>
                        !x.Eliminado &&
                        x.NombreIngreso == cuentaIngreso.Id &&
                        x.Descripcion != null &&
                        x.Descripcion.Contains(factura.NumeroFactura)
                    );

                    if (!yaExisteIngreso)
                    {
                        _context.FichaIngresos.Add(new FichaIngreso
                        {
                            NombreIngreso = cuentaIngreso.Id,
                            Fecha = factura.Fecha,
                            Mes = factura.Fecha.ToString("MMMM", new CultureInfo("es-ES")),
                            Descripcion = $"Factura {factura.NumeroFactura} - {factura.Cliente} - {descripcionOriginal}",
                            Importe = importe,
                            Eliminado = false,
                            FechaEliminacion = null
                        });
                    }

                    break;
                }
            }

            if (debeCrearAlertaAceite)
            {
                var yaExisteAlerta = await _context.AlertasClientes.AnyAsync(x =>
                    !x.Eliminado &&
                    x.IdFacturaEmitida == factura.Id
                );

                if (!yaExisteAlerta)
                {
                    _context.AlertasClientes.Add(new AlertaCliente
                    {
                        Cliente = factura.Cliente,
                        Telefono = factura.TelefonoCliente,
                        Mensaje = $"Llamar al cliente {factura.Cliente} al movil {factura.TelefonoCliente} para indicarle que le toca Servicio de cambio de aceite y filtro.",
                        FechaAviso = DateTime.UtcNow.AddMonths(1),
                        Atendida = false,
                        IdFacturaEmitida = factura.Id,
                        Eliminado = false
                    });
                }
            }
        }

        private static List<FacturaItemDTO> NormalizeItems(List<FacturaItemDTO>? items, string? itemsJson)
        {
            var source = items is { Count: > 0 } ? items : DeserializeItems(itemsJson);

            return source
                .Where(x => !string.IsNullOrWhiteSpace(x.Descripcion))
                .Select(x => new FacturaItemDTO
                {
                    Descripcion = x.Descripcion?.Trim(),
                    Cantidad = x.Cantidad <= 0 ? 1 : Round2(x.Cantidad),
                    Importe = Round2(x.Importe)
                })
                .Where(x => x.Importe >= 0)
                .ToList();
        }

        private static List<FacturaItemDTO> DeserializeItems(string? itemsJson)
        {
            if (string.IsNullOrWhiteSpace(itemsJson))
                return new List<FacturaItemDTO>();

            try
            {
                return JsonSerializer.Deserialize<List<FacturaItemDTO>>(
                    itemsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<FacturaItemDTO>();
            }
            catch
            {
                return new List<FacturaItemDTO>();
            }
        }

        private static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            var normalized = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);

            var chars = normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();

            var sinAcentos = new string(chars).Normalize(NormalizationForm.FormC);

            return Regex.Replace(sinAcentos, @"\s+", " ");
        }

        [HttpGet("numero/{numeroFactura}")]
        public async Task<ActionResult> GetByNumero(string numeroFactura)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var factura = await _context.FacturasEmitidas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.NumeroFactura == numeroFactura &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                if (factura == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe una factura con ese numero.";
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
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var factura = await _context.FacturasEmitidas
                    .AsNoTracking()
                    .Where(x =>
                        x.IdOrdenTrabajo == idOrden &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
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

        private static string FormatNumeroFactura(string serie, string ownerKey, int numero, int anio)
        {
            var ownerSegment = ownerKey.Replace(" ", "").ToUpperInvariant();
            return $"{serie}-{anio}-{ownerSegment}-{numero:D4}";
        }

        private static decimal Round2(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }

    public class FacturaEmitidaCreateDto
    {
        public string? NumeroFactura { get; set; }
        public string? Serie { get; set; }
        public int? IdOrdenTrabajo { get; set; }
        public DateTime? Fecha { get; set; }
        public string Cliente { get; set; } = null!;
        public string? Dni { get; set; }
        public string? DireccionCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? Matricula { get; set; }
        public string? Km { get; set; }
        public string? Observaciones { get; set; }
        public int IvaPct { get; set; } = 21;
        public decimal Otros { get; set; }
        public List<FacturaItemDTO>? Items { get; set; }
        public string? ItemsJson { get; set; }
    }

    public class FacturaItemDTO
    {
        public string? Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Importe { get; set; }
    }
}
