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
                TipoPago = dto.TipoPago,
                TotalAbonado = dto.TotalAbonado,
                FechaVencimiento = dto.FechaVencimiento,
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
                var baseAntesOtros = Round2(items.Sum(x => x.Cantidad * x.Importe));
                var subtotal = Round2(Math.Max(0, baseAntesOtros - otros));
                var iva = ivaPct > 0
                    ? Round2(subtotal * (ivaPct / 100m))
                    : 0m;
                var total = Round2(subtotal + iva);
                var tipoPago = NormalizeTipoPago(dto.TipoPago);
                var totalFactura = total;
                var totalAbonado = tipoPago == "Contado"
                    ? totalFactura
                    : Round2(Math.Clamp(dto.TotalAbonado ?? 0m, 0m, totalFactura));
                var saldoPendiente = Round2(Math.Max(0m, totalFactura - totalAbonado));
                DateTime? fechaVencimiento = tipoPago == "Credito"
                    ? (dto.FechaVencimiento?.Date ?? fecha.Date.AddDays(NormalizePlazoCredito(dto.PlazoCreditoDias)))
                    : null;
                var estadoCxC = GetEstadoCxC(totalAbonado, saldoPendiente);
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
                    TotalFactura = totalFactura,
                    TotalAbonado = totalAbonado,
                    SaldoPendiente = saldoPendiente,
                    FechaVencimiento = fechaVencimiento,
                    TipoPago = tipoPago,
                    EstadoCxC = estadoCxC,
                    Observaciones = dto.Observaciones?.Trim(),
                    ItemsJson = itemsJson,
                    Eliminado = false
                };

                _context.FacturasEmitidas.Add(factura);
                _context.Entry(factura).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                await CrearIngresoAutomaticoSiAplica(factura, items, workshopId.Value);
                await CrearRentabilidadRepuestosFacturados(factura, items, workshopId.Value);

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
                    factura.TotalFactura,
                    factura.TotalAbonado,
                    factura.SaldoPendiente,
                    factura.FechaVencimiento,
                    factura.TipoPago,
                    factura.EstadoCxC,
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
            var serviciosFrecuentes = await _context.ServiciosFrecuentes
                .AsNoTracking()
                .Where(x =>
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId
                )
                .Select(x => x.Nombre)
                .ToListAsync();
            var serviciosFrecuentesNormalizados = serviciosFrecuentes
                .Select(NormalizarTexto)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet();

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
                {
                    nombreCuenta = serviciosFrecuentesNormalizados.Contains(descLower)
                        ? "Servicio"
                        : "Ventas";
                }

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
                        FechaAviso = DateTime.UtcNow.AddMonths(8),
                        Atendida = false,
                        IdFacturaEmitida = factura.Id,
                        Eliminado = false
                    };

                    _context.AlertasClientes.Add(alerta);
                    _context.Entry(alerta).Property("WorkshopId").CurrentValue = workshopId;
                }
            }
        }

        private async Task CrearRentabilidadRepuestosFacturados(FacturaEmitida factura, List<FacturaItemDTO> items, int workshopId)
        {
            foreach (var item in items)
            {
                RepuestoStock? repuestoBase = null;
                var repuestoBaseId = item.RepuestoStockId ?? item.IdRepuesto;

                if (repuestoBaseId.HasValue)
                {
                    repuestoBase = await _context.RepuestosStock
                        .Include(x => x.Proveedor)
                        .FirstOrDefaultAsync(x =>
                            x.Id == repuestoBaseId.Value &&
                            !x.Eliminado &&
                            !x.EsFacturado &&
                            EF.Property<int>(x, "WorkshopId") == workshopId
                        );
                }

                if (repuestoBase == null && !string.IsNullOrWhiteSpace(item.Descripcion))
                {
                    var descripcion = item.Descripcion.Trim();
                    repuestoBase = await _context.RepuestosStock
                        .Include(x => x.Proveedor)
                        .FirstOrDefaultAsync(x =>
                            x.Nombre == descripcion &&
                            !x.Eliminado &&
                            !x.EsFacturado &&
                            EF.Property<int>(x, "WorkshopId") == workshopId
                        );
                }

                var proveedorId = item.IdProveedor ?? repuestoBase?.IdProveedor;
                string? nombreProveedor = item.NombreProveedor?.Trim();

                if (string.IsNullOrWhiteSpace(nombreProveedor))
                    nombreProveedor = repuestoBase?.Proveedor?.Nombre;

                if (string.IsNullOrWhiteSpace(nombreProveedor) && proveedorId.HasValue)
                {
                    nombreProveedor = await _context.Proveedores
                        .Where(x =>
                            x.Id == proveedorId.Value &&
                            !x.Eliminado &&
                            EF.Property<int>(x, "WorkshopId") == workshopId
                        )
                        .Select(x => x.Nombre)
                        .FirstOrDefaultAsync();
                }

                var precioCompra = Round2(item.PrecioCompra ?? repuestoBase?.PrecioCompra ?? 0m);
                var precioVenta = Round2(item.Importe);
                var categoria = EsLineaRepuesto(item) || repuestoBase != null
                    ? "Repuesto facturado"
                    : NormalizarTexto(item.Kind ?? item.Tipo ?? "") == "labor"
                        ? "Mano de obra facturada"
                        : "Concepto facturado";

                var repuestoFacturado = new RepuestoStock
                {
                    Nombre = (item.Descripcion ?? repuestoBase?.Nombre ?? "Concepto facturado").Trim(),
                    CodigoReferencia = repuestoBase?.CodigoReferencia,
                    Marca = repuestoBase?.Marca,
                    Categoria = categoria,
                    Cantidad = item.Cantidad,
                    StockMinimo = 0,
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta,
                    Ubicacion = null,
                    Observaciones = $"Factura {factura.NumeroFactura}",
                    IdProveedor = proveedorId,
                    NombreProveedorSnapshot = nombreProveedor,
                    EsFacturado = true,
                    IdFacturaEmitida = factura.Id,
                    NumeroFactura = factura.NumeroFactura,
                    FechaFactura = factura.Fecha,
                    Cliente = factura.Cliente,
                    Matricula = factura.Matricula,
                    Eliminado = false
                };

                _context.RepuestosStock.Add(repuestoFacturado);
                _context.Entry(repuestoFacturado).Property("WorkshopId").CurrentValue = workshopId;
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
                    Importe = Round2(x.Importe),
                    Tipo = x.Tipo?.Trim(),
                    Kind = x.Kind?.Trim(),
                    RepuestoStockId = x.RepuestoStockId,
                    IdRepuesto = x.IdRepuesto,
                    IdProveedor = x.IdProveedor,
                    NombreProveedor = x.NombreProveedor?.Trim(),
                    PrecioCompra = x.PrecioCompra.HasValue ? Round2(x.PrecioCompra.Value) : null
                })
                .Where(x => Round2(x.Cantidad * x.Importe) > 0)
                .ToList();
        }

        private static bool EsLineaRepuesto(FacturaItemDTO item)
        {
            var tipo = NormalizarTexto(item.Tipo ?? item.Kind ?? "");
            if (tipo is "repuesto" or "repuestos" or "part" or "parts")
                return true;

            if (item.RepuestoStockId.HasValue || item.IdRepuesto.HasValue || item.PrecioCompra.HasValue || item.IdProveedor.HasValue)
                return true;

            var desc = NormalizarTexto(item.Descripcion ?? "");
            return desc.Contains("repuesto");
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

        [HttpGet("cxc")]
        public async Task<ActionResult> GetCuentasPorCobrar([FromQuery] string? estado = null)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var query = _context.FacturasEmitidas
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value &&
                        (x.TipoPago == "Credito" || x.SaldoPendiente > 0)
                    );

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    var estadoNorm = NormalizeEstadoCxC(estado);
                    query = query.Where(x => x.EstadoCxC == estadoNorm);
                }

                var items = await query
                    .OrderBy(x => x.EstadoCxC == "Pagada")
                    .ThenBy(x => x.FechaVencimiento ?? DateTime.MaxValue)
                    .ThenByDescending(x => x.Fecha)
                    .Select(x => new
                    {
                        x.Id,
                        x.NumeroFactura,
                        x.Fecha,
                        x.Cliente,
                        x.Dni,
                        x.TelefonoCliente,
                        x.Matricula,
                        x.TotalFactura,
                        x.TotalAbonado,
                        x.SaldoPendiente,
                        x.FechaVencimiento,
                        x.TipoPago,
                        x.EstadoCxC
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Cuentas por cobrar";
                respuesta.Data.Add(items);
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpPut("{id:int}/abono")]
        public async Task<ActionResult> RegistrarAbono(int id, [FromBody] FacturaAbonoDto dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var factura = await _context.FacturasEmitidas.FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value
                );

                if (factura == null)
                    return NotFound(new { message = "No existe la factura." });

                var abono = Round2(dto.Importe);
                if (abono <= 0)
                    return BadRequest(new { message = "El abono debe ser mayor que 0." });

                var totalFactura = factura.TotalFactura > 0 ? factura.TotalFactura : factura.Total;
                factura.TotalFactura = totalFactura;
                factura.TotalAbonado = Round2(Math.Min(totalFactura, factura.TotalAbonado + abono));
                factura.SaldoPendiente = Round2(Math.Max(0m, totalFactura - factura.TotalAbonado));
                factura.EstadoCxC = GetEstadoCxC(factura.TotalAbonado, factura.SaldoPendiente);

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Abono registrado.";
                respuesta.Data.Add(new
                {
                    factura.Id,
                    factura.TotalFactura,
                    factura.TotalAbonado,
                    factura.SaldoPendiente,
                    factura.EstadoCxC
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

        private static string NormalizeTipoPago(string? tipoPago)
        {
            var clean = NormalizarTexto(tipoPago ?? "Contado");
            return clean == "credito" ? "Credito" : "Contado";
        }

        private static int NormalizePlazoCredito(int? plazo)
        {
            return plazo == 60 ? 60 : 30;
        }

        private static string NormalizeEstadoCxC(string? estado)
        {
            var clean = NormalizarTexto(estado ?? "");
            return clean switch
            {
                "pagada" => "Pagada",
                "parcial" => "Parcial",
                _ => "Pendiente"
            };
        }

        private static string GetEstadoCxC(decimal totalAbonado, decimal saldoPendiente)
        {
            if (saldoPendiente <= 0.009m) return "Pagada";
            return totalAbonado > 0.009m ? "Parcial" : "Pendiente";
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
                lines.Add($"{item.Descripcion} | Cant.: {item.Cantidad:0.##} | Precio unitario: {item.Importe:0.00} EUR | Importe: {totalLinea:0.00} EUR");
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
        public string? TipoPago { get; set; }
        public decimal? TotalAbonado { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int? PlazoCreditoDias { get; set; }
        public List<FacturaItemDTO>? Items { get; set; }
        public string? ItemsJson { get; set; }
    }

    public class FacturaAbonoDto
    {
        public decimal Importe { get; set; }
    }

    public class FacturaItemDTO
    {
        public string? Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Importe { get; set; }
        public string? Tipo { get; set; }
        public string? Kind { get; set; }
        public int? RepuestoStockId { get; set; }
        public int? IdRepuesto { get; set; }
        public int? IdProveedor { get; set; }
        public string? NombreProveedor { get; set; }
        public decimal? PrecioCompra { get; set; }
    }
}
