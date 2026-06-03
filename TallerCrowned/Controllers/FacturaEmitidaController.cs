using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.IO.Compression;
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
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public FacturaEmitidaController(
            dbContext context,
            ICurrentUserService currentUserService,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _currentWorkshopService = currentWorkshopService;
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

                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();
                var ownerKey = GetOwnerKey();
                var serie = NormalizeSerie(dto.Serie);
                var anio = DateTime.Now.Year;
                var fecha = dto.Fecha ?? DateTime.UtcNow;
                var items = NormalizeItems(dto.Items, dto.ItemsJson);

                if (items.Count == 0)
                    return BadRequest(new { message = "La factura debe tener al menos una linea con importe mayor que 0." });

                if (dto.IdOrdenTrabajo.HasValue)
                {
                    var ordenExiste = await _context.OrdenesTrabajo.AnyAsync(x =>
                        x.Id == dto.IdOrdenTrabajo.Value &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                    if (!ordenExiste)
                        return NotFound(new { message = "No existe la orden o no pertenece al usuario actual." });
                }

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                var numeroFactura = allowProvidedNumber && !string.IsNullOrWhiteSpace(dto.NumeroFactura)
                    ? dto.NumeroFactura.Trim()
                    : await GenerateNumeroFactura(workshopId.Value, ownerKey, serie, anio);

                var existe = await _context.FacturasEmitidas.AnyAsync(x =>
                    x.NumeroFactura == numeroFactura &&
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value
                );

                if (existe)
                    return BadRequest(new { message = "Ya existe una factura emitida con ese numero." });

                var ivaPct = dto.IvaPct <= 0 ? 21 : dto.IvaPct;
                var otros = Round2(dto.Otros);
                var totalConIva = Round2(items.Sum(x => x.Cantidad * x.Importe));
                var total = Round2(Math.Max(0, totalConIva - otros));
                var subtotal = ivaPct > 0
                    ? Round2(total / (1 + (ivaPct / 100m)))
                    : total;
                var iva = Round2(total - subtotal);
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
                _context.Entry(factura).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                await CrearIngresoAutomaticoSiAplica(factura, items, workshopId.Value);

                if (factura.IdOrdenTrabajo.HasValue)
                {
                    var orden = await _context.OrdenesTrabajo.FirstOrDefaultAsync(x =>
                        x.Id == factura.IdOrdenTrabajo.Value &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
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

        private async Task<string> GenerateNumeroFactura(int workshopId, string ownerKey, string serie, int anio)
        {
            var numerador = await _context.NumeradoresFactura.FirstOrDefaultAsync(x =>
                x.WorkshopId == workshopId &&
                x.Serie == serie &&
                x.Anio == anio
            );

            if (numerador == null)
            {
                numerador = new NumeradorFactura
                {
                    WorkshopId = workshopId,
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

            return FormatNumeroFactura(serie, workshopId, numerador.UltimoNumero, anio);
        }

        private async Task CrearIngresoAutomaticoSiAplica(FacturaEmitida factura, List<FacturaItemDTO> items, int workshopId)
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

                ["Servicios a terceros"] = d =>
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

                var nombreCuenta = reglas
                    .FirstOrDefault(regla => regla.Value(descLower))
                    .Key;

                if (string.IsNullOrWhiteSpace(nombreCuenta))
                    nombreCuenta = "Ventas";

                if (nombreCuenta == "Servicio cambio de aceite y filtro")
                    debeCrearAlertaAceite = true;

                var cuentaIngreso = await _context.Ingresos
                    .FirstOrDefaultAsync(x =>
                        x.NombreIngreso.ToLower() == nombreCuenta.ToLower() &&
                        EF.Property<int>(x, "WorkshopId") == workshopId
                    );

                if (cuentaIngreso == null)
                {
                    cuentaIngreso = new Ingreso
                    {
                        NombreIngreso = nombreCuenta
                    };

                    _context.Ingresos.Add(cuentaIngreso);
                    _context.Entry(cuentaIngreso).Property("WorkshopId").CurrentValue = workshopId;
                    await _context.SaveChangesAsync();
                }

                var importe = Round2(item.Cantidad * item.Importe);

                var yaExisteIngreso = await _context.FichaIngresos.AnyAsync(x =>
                    !x.Eliminado &&
                    x.NombreIngreso == cuentaIngreso.Id &&
                    EF.Property<int>(x, "WorkshopId") == workshopId &&
                    x.Descripcion != null &&
                    x.Descripcion.Contains(factura.NumeroFactura)
                );

                if (!yaExisteIngreso)
                {
                    var fichaIngreso = new FichaIngreso
                    {
                        NombreIngreso = cuentaIngreso.Id,
                        Fecha = factura.Fecha,
                        Mes = factura.Fecha.ToString("MMMM", new CultureInfo("es-ES")),
                        Descripcion = $"Factura {factura.NumeroFactura} - {factura.Cliente} - {descripcionOriginal}",
                        Importe = importe,
                        Eliminado = false,
                        FechaEliminacion = null
                    };

                    _context.FichaIngresos.Add(fichaIngreso);
                    _context.Entry(fichaIngreso).Property("WorkshopId").CurrentValue = workshopId;
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
                    var alerta = new AlertaCliente
                    {
                        Cliente = factura.Cliente,
                        Telefono = factura.TelefonoCliente,
                        Mensaje = $"Llamar al cliente {factura.Cliente} al movil {factura.TelefonoCliente} para indicarle que le toca Servicio de cambio de aceite y filtro.",
                        FechaAviso = DateTime.UtcNow.AddMonths(1),
                        Atendida = false,
                        IdFacturaEmitida = factura.Id,
                        Eliminado = false
                    };

                    _context.AlertasClientes.Add(alerta);
                    _context.Entry(alerta).Property("WorkshopId").CurrentValue = workshopId;
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
                .Where(x => Round2(x.Cantidad * x.Importe) > 0)
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
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var factura = await _context.FacturasEmitidas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.NumeroFactura == numeroFactura &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
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
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var factura = await _context.FacturasEmitidas
                    .AsNoTracking()
                    .Where(x =>
                        x.IdOrdenTrabajo == idOrden &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
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

        [HttpGet("exportar")]
        public async Task<ActionResult> ExportarPorPeriodo([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            if (fechaFin.Date < fechaInicio.Date)
                return BadRequest(new { message = "La fecha fin no puede ser menor que la fecha inicio." });

            var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
            if (!workshopId.HasValue) return Forbid();

            var desde = fechaInicio.Date;
            var hastaExcl = fechaFin.Date.AddDays(1);

            var facturas = await _context.FacturasEmitidas
                .AsNoTracking()
                .Where(x =>
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value &&
                    x.Fecha >= desde &&
                    x.Fecha < hastaExcl)
                .OrderBy(x => x.Fecha)
                .ThenBy(x => x.NumeroFactura)
                .ToListAsync();

            if (facturas.Count == 0)
                return NotFound(new { message = "No hay facturas emitidas en el periodo seleccionado." });

            await using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var factura in facturas)
                {
                    var entryName = $"{SafeFileName(factura.NumeroFactura)}_{factura.Fecha:yyyyMMdd}.pdf";
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);

                    await using var entryStream = entry.Open();
                    var pdfBytes = BuildInvoicePdf(factura);
                    await entryStream.WriteAsync(pdfBytes);
                }
            }

            var fileName = $"facturas-{fechaInicio:yyyyMMdd}-a-{fechaFin:yyyyMMdd}.zip";
            return File(zipStream.ToArray(), "application/zip", fileName);
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

        private static decimal Round2(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static byte[] BuildInvoicePdf(FacturaEmitida factura)
        {
            var items = DeserializeItems(factura.ItemsJson);
            var ivaPct = factura.Subtotal > 0
                ? Math.Round((factura.Iva / factura.Subtotal) * 100, 2)
                : 0;

            var lines = new List<string>
            {
                "FACTURA",
                $"Numero: {factura.NumeroFactura}",
                $"Fecha: {factura.Fecha:dd/MM/yyyy}",
                "",
                "Cliente",
                $"Nombre: {factura.Cliente}",
                $"DNI/NIE/NIF: {factura.Dni ?? ""}",
                $"Direccion: {factura.DireccionCliente ?? ""}",
                $"Telefono: {factura.TelefonoCliente ?? ""}",
                $"Referencia: {factura.Matricula ?? ""}",
                $"Km: {factura.Km ?? ""}",
                "",
                "Conceptos"
            };

            foreach (var item in items)
            {
                var totalLinea = Round2(item.Cantidad * item.Importe);
                lines.Add($"{item.Descripcion} | Cant.: {item.Cantidad:0.##} | Importe IVA incl.: {totalLinea:0.00} EUR");
            }

            lines.Add("");
            lines.Add($"Base imponible: {factura.Subtotal:0.00} EUR");
            lines.Add($"IVA ({ivaPct:0.##}%): {factura.Iva:0.00} EUR");
            if (factura.Otros > 0)
                lines.Add($"Otros/descuento: -{factura.Otros:0.00} EUR");
            lines.Add($"TOTAL: {factura.Total:0.00} EUR");

            if (!string.IsNullOrWhiteSpace(factura.Observaciones))
            {
                lines.Add("");
                lines.Add("Observaciones");
                lines.Add(factura.Observaciones);
            }

            return SimplePdf(lines);
        }

        private static byte[] SimplePdf(IReadOnlyList<string> lines)
        {
            var content = new StringBuilder();
            content.AppendLine("BT");
            content.AppendLine("/F1 11 Tf");
            content.AppendLine("50 790 Td");

            foreach (var rawLine in lines)
            {
                foreach (var line in WrapLine(rawLine ?? "", 92))
                {
                    content.AppendLine($"({PdfEscape(line)}) Tj");
                    content.AppendLine("0 -16 Td");
                }
            }

            content.AppendLine("ET");

            var contentBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(content.ToString());
            var objects = new List<byte[]>
            {
                Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"),
                Encoding.ASCII.GetBytes("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"),
                Encoding.ASCII.GetBytes("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>"),
                Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"),
                Combine(
                    Encoding.ASCII.GetBytes($"<< /Length {contentBytes.Length} >>\nstream\n"),
                    contentBytes,
                    Encoding.ASCII.GetBytes("\nendstream"))
            };

            using var ms = new MemoryStream();
            WriteAscii(ms, "%PDF-1.4\n");
            var offsets = new List<long> { 0 };

            for (var i = 0; i < objects.Count; i++)
            {
                offsets.Add(ms.Position);
                WriteAscii(ms, $"{i + 1} 0 obj\n");
                ms.Write(objects[i]);
                WriteAscii(ms, "\nendobj\n");
            }

            var xref = ms.Position;
            WriteAscii(ms, $"xref\n0 {objects.Count + 1}\n");
            WriteAscii(ms, "0000000000 65535 f \n");
            foreach (var offset in offsets.Skip(1))
                WriteAscii(ms, $"{offset:0000000000} 00000 n \n");
            WriteAscii(ms, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");

            return ms.ToArray();
        }

        private static IEnumerable<string> WrapLine(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
            {
                yield return "";
                yield break;
            }

            for (var i = 0; i < text.Length; i += maxLength)
                yield return text.Substring(i, Math.Min(maxLength, text.Length - i));
        }

        private static string PdfEscape(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }

        private static string SafeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var clean = new string((value ?? "factura").Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            return string.IsNullOrWhiteSpace(clean) ? "factura" : clean;
        }

        private static byte[] Combine(params byte[][] parts)
        {
            using var ms = new MemoryStream();
            foreach (var part in parts)
                ms.Write(part);
            return ms.ToArray();
        }

        private static void WriteAscii(Stream stream, string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            stream.Write(bytes);
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
