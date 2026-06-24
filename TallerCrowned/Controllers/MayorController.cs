using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MayorController : ControllerBase
    {
        private static readonly string[] CuentasPermitidas = { "Cliente", "Proveedor", "Banco" };
        private static readonly string[] TiposPermitidos = { "Ingreso", "Egreso" };

        private readonly dbContext _context;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public MayorController(dbContext context, ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpGet]
        public async Task<ActionResult> Get(
            [FromQuery] string? cuenta,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int? bankAccountId)
        {
            var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
            if (!workshopId.HasValue) return Forbid();

            var enabled = await _context.Workshops
                .AsNoTracking()
                .Where(x => x.Id == workshopId.Value)
                .Select(x => x.EnableLedger)
                .FirstOrDefaultAsync();

            if (!enabled)
                return Ok(new Respuesta<object> { Ok = 0, Message = "Mayor no esta activo para este taller." });

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin < fechaInicio)
                return BadRequest(new { message = "La fecha fin no puede ser menor que la fecha inicio." });

            var normalizedCuenta = NormalizeCuenta(cuenta, allowEmpty: true);
            var start = fechaInicio?.Date;
            var endExcl = fechaFin?.Date.AddDays(1);

            var manualQuery = _context.MayorMovimientos
                .AsNoTracking()
                .Where(x =>
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value &&
                    (!start.HasValue || x.Fecha >= start.Value) &&
                    (!endExcl.HasValue || x.Fecha < endExcl.Value));

            var manualItems = await manualQuery
                .Select(x => new MayorLedgerItem
                {
                    Id = $"mayor-{x.Id}",
                    Source = "Mayor",
                    SourceId = x.Id,
                    Cuenta = x.Cuenta,
                    TipoMovimiento = x.TipoMovimiento,
                    Fecha = x.Fecha,
                    Referencia = x.Referencia,
                    Descripcion = x.Descripcion,
                    Importe = x.Importe
                })
                .ToListAsync();

            var facturaBaseItems = await _context.FacturasEmitidas
                .AsNoTracking()
                .Where(x =>
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value &&
                    (!start.HasValue || x.Fecha >= start.Value) &&
                    (!endExcl.HasValue || x.Fecha < endExcl.Value))
                .Select(x => new
                {
                    x.Id,
                    x.NumeroFactura,
                    x.Fecha,
                    x.Cliente,
                    x.Subtotal,
                    x.Iva,
                    x.Total,
                    x.TotalFactura,
                    x.TotalAbonado,
                    x.TipoPago,
                    x.BankAccountId,
                    x.BankAccountName,
                    x.BankAccountIban
                })
                .ToListAsync();

            var facturaItems = facturaBaseItems
                .SelectMany(f =>
                {
                    var totalFactura = f.TotalFactura > 0 ? f.TotalFactura : f.Total;
                    var esCredito = string.Equals(f.TipoPago, "Credito", StringComparison.OrdinalIgnoreCase);
                    var abonado = esCredito
                        ? Math.Min(totalFactura, f.TotalAbonado)
                        : totalFactura;
                    var baseAbonada = esCredito
                        ? totalFactura == 0
                            ? 0
                            : Round2(f.Subtotal * abonado / totalFactura)
                        : f.Subtotal;
                    var rows = new List<MayorLedgerItem>();

                    if (baseAbonada != 0)
                    {
                        rows.Add(new MayorLedgerItem
                        {
                            Id = $"factura-cliente-{f.Id}",
                            Source = "Factura",
                            SourceId = f.Id,
                            Cuenta = "Cliente",
                            TipoMovimiento = "Ingreso",
                            Fecha = f.Fecha,
                            Referencia = f.NumeroFactura,
                            Descripcion = $"Abono factura {f.NumeroFactura} - {f.Cliente}",
                            Importe = baseAbonada
                        });
                    }

                    if (abonado != 0)
                    {
                        rows.Add(new MayorLedgerItem
                        {
                            Id = $"factura-banco-{f.Id}",
                            Source = "Factura",
                            SourceId = f.Id,
                            Cuenta = "Banco",
                            TipoMovimiento = "Ingreso",
                            Fecha = f.Fecha,
                            Referencia = f.NumeroFactura,
                            Descripcion = $"Cobro factura {f.NumeroFactura} - {f.Cliente}",
                            Importe = abonado,
                            BankAccountId = esCredito ? null : f.BankAccountId,
                            BankAccountName = esCredito ? null : f.BankAccountName,
                            BankAccountIban = esCredito ? null : f.BankAccountIban
                        });
                    }

                    return rows;
                })
                .ToList();

            var invoiceNumbers = facturaBaseItems
                .Select(x => x.NumeroFactura)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var ingresoBaseItems = await _context.FichaIngresos
                .AsNoTracking()
                .Where(x =>
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value &&
                    (!start.HasValue || (x.Fecha.HasValue && x.Fecha.Value >= start.Value)) &&
                    (!endExcl.HasValue || (x.Fecha.HasValue && x.Fecha.Value < endExcl.Value)))
                .Join(
                    _context.Ingresos.AsNoTracking(),
                    f => f.NombreIngreso,
                    i => i.Id,
                    (f, i) => new
                    {
                        f.Id,
                        f.Fecha,
                        Tipo = i.NombreIngreso,
                        f.Descripcion,
                        f.Importe,
                        f.BankAccountId,
                        f.BankAccountName,
                        f.BankAccountIban
                    })
                .ToListAsync();

            var ingresoItems = ingresoBaseItems
                .Where(x => !invoiceNumbers.Any(n =>
                    !string.IsNullOrWhiteSpace(x.Descripcion) &&
                    x.Descripcion.Contains(n, StringComparison.OrdinalIgnoreCase)))
                .SelectMany(x =>
                {
                    var rows = new List<MayorLedgerItem>();
                    var descripcion = string.IsNullOrWhiteSpace(x.Descripcion)
                        ? x.Tipo
                        : $"{x.Tipo} - {x.Descripcion}";

                    rows.Add(new MayorLedgerItem
                    {
                        Id = $"ingreso-cliente-{x.Id}",
                        Source = "Ingreso",
                        SourceId = x.Id,
                        Cuenta = "Cliente",
                        TipoMovimiento = "Ingreso",
                        Fecha = x.Fecha ?? DateTime.MinValue,
                        Referencia = $"ING-{x.Id}",
                        Descripcion = descripcion,
                        Importe = x.Importe
                    });

                    rows.Add(new MayorLedgerItem
                    {
                        Id = $"ingreso-banco-{x.Id}",
                        Source = "Ingreso",
                        SourceId = x.Id,
                        Cuenta = "Banco",
                        TipoMovimiento = "Ingreso",
                        Fecha = x.Fecha ?? DateTime.MinValue,
                        Referencia = $"ING-{x.Id}",
                        Descripcion = descripcion,
                        Importe = AddIva(x.Importe),
                        BankAccountId = x.BankAccountId,
                        BankAccountName = x.BankAccountName,
                        BankAccountIban = x.BankAccountIban
                    });

                    return rows;
                })
                .ToList();

            var egresoBaseItems = await _context.FichaEgresos
                .AsNoTracking()
                .Where(x =>
                    !x.Eliminado &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value &&
                    (!start.HasValue || (x.Fecha.HasValue && x.Fecha.Value >= start.Value)) &&
                    (!endExcl.HasValue || (x.Fecha.HasValue && x.Fecha.Value < endExcl.Value)))
                .Join(
                    _context.Egresos.AsNoTracking(),
                    f => f.NombreEgreso,
                    e => e.Id,
                    (f, e) => new
                    {
                        f.Id,
                        f.Fecha,
                        Tipo = e.Nombre,
                        f.Descripcion,
                        f.Importe,
                        f.BankAccountId,
                        f.BankAccountName,
                        f.BankAccountIban
                    })
                .ToListAsync();

            var egresoItems = egresoBaseItems
                .SelectMany(x =>
                {
                    var rows = new List<MayorLedgerItem>();
                    var descripcion = string.IsNullOrWhiteSpace(x.Descripcion)
                        ? x.Tipo
                        : $"{x.Tipo} - {x.Descripcion}";

                    rows.Add(new MayorLedgerItem
                    {
                        Id = $"egreso-proveedor-{x.Id}",
                        Source = "Egreso",
                        SourceId = x.Id,
                        Cuenta = "Proveedor",
                        TipoMovimiento = "Egreso",
                        Fecha = x.Fecha ?? DateTime.MinValue,
                        Referencia = $"EGR-{x.Id}",
                        Descripcion = descripcion,
                        Importe = x.Importe
                    });

                    rows.Add(new MayorLedgerItem
                    {
                        Id = $"egreso-banco-{x.Id}",
                        Source = "Egreso",
                        SourceId = x.Id,
                        Cuenta = "Banco",
                        TipoMovimiento = "Egreso",
                        Fecha = x.Fecha ?? DateTime.MinValue,
                        Referencia = $"EGR-{x.Id}",
                        Descripcion = descripcion,
                        Importe = AddIva(x.Importe),
                        BankAccountId = x.BankAccountId,
                        BankAccountName = x.BankAccountName,
                        BankAccountIban = x.BankAccountIban
                    });

                    return rows;
                })
                .ToList();

            var allItems = manualItems
                .Concat(facturaItems)
                .Concat(ingresoItems)
                .Concat(egresoItems)
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.SourceId)
                .ToList();

            var filteredItems = allItems
                .Where(x => !bankAccountId.HasValue || x.Cuenta != "Banco" || x.BankAccountId == bankAccountId.Value)
                .ToList();

            var items = filteredItems
                .Where(x => normalizedCuenta == "" || x.Cuenta == normalizedCuenta)
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.SourceId)
                .ToList();

            var resumen = filteredItems
                .GroupBy(x => x.Cuenta)
                .Select(g => new
                {
                    Cuenta = g.Key,
                    Ingresos = g.Where(x => x.TipoMovimiento == "Ingreso").Sum(x => x.Importe),
                    Egresos = g.Where(x => x.TipoMovimiento == "Egreso").Sum(x => x.Importe),
                    Saldo = g.Sum(x => x.TipoMovimiento == "Ingreso" ? x.Importe : -x.Importe)
                })
                .ToList();

            return Ok(new Respuesta<object>
            {
                Ok = 1,
                Message = "Movimientos del mayor",
                Data = { new { items, resumen } }
            });
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] MayorMovimientoDto dto)
        {
            var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
            if (!workshopId.HasValue) return Forbid();

            var enabled = await _context.Workshops
                .Where(x => x.Id == workshopId.Value)
                .Select(x => x.EnableLedger)
                .FirstOrDefaultAsync();

            if (!enabled)
                return BadRequest(new { message = "Mayor no esta activo para este taller." });

            var cuenta = NormalizeCuenta(dto.Cuenta);
            var tipo = NormalizeTipo(dto.TipoMovimiento, cuenta);

            if (dto.Importe <= 0)
                return BadRequest(new { message = "El importe debe ser mayor que 0." });

            if (string.IsNullOrWhiteSpace(dto.Referencia))
                return BadRequest(new { message = "La referencia es requerida." });

            if (cuenta == "Proveedor" && tipo == "Egreso")
            {
                var fecha = dto.Fecha?.Date ?? DateTime.Now.Date;
                var egreso = await _context.Egresos.FirstOrDefaultAsync(x =>
                    x.Nombre == "Proveedores" &&
                    EF.Property<int>(x, "WorkshopId") == workshopId.Value);

                if (egreso == null)
                {
                    egreso = new Egreso
                    {
                        Nombre = "Proveedores",
                        TipoGasto = "variable"
                    };
                    _context.Egresos.Add(egreso);
                    _context.Entry(egreso).Property("WorkshopId").CurrentValue = workshopId.Value;
                    await _context.SaveChangesAsync();
                }

                var fichaEgreso = new FichaEgreso
                {
                    Fecha = fecha,
                    Mes = fecha.ToString("MMMM", new CultureInfo("es-ES")),
                    NombreEgreso = egreso.Id,
                    Descripcion = $"{dto.Referencia.Trim()} - {(string.IsNullOrWhiteSpace(dto.Descripcion) ? "Movimiento proveedor" : dto.Descripcion.Trim())}",
                    Importe = dto.Importe,
                    Eliminado = false,
                    FechaEliminacion = null
                };

                _context.FichaEgresos.Add(fichaEgreso);
                _context.Entry(fichaEgreso).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                return Ok(new Respuesta<object>
                {
                    Ok = 1,
                    Message = "Egreso de proveedor registrado.",
                    Data = { new { fichaEgreso.Id } }
                });
            }

            var movimiento = new MayorMovimiento
            {
                Cuenta = cuenta,
                TipoMovimiento = tipo,
                Fecha = dto.Fecha?.Date ?? DateTime.Now.Date,
                Referencia = dto.Referencia.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                Importe = dto.Importe,
                Eliminado = false
            };

            _context.MayorMovimientos.Add(movimiento);
            _context.Entry(movimiento).Property("WorkshopId").CurrentValue = workshopId.Value;
            await _context.SaveChangesAsync();

            return Ok(new Respuesta<object>
            {
                Ok = 1,
                Message = "Movimiento registrado en el mayor.",
                Data = { new { movimiento.Id } }
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
            if (!workshopId.HasValue) return Forbid();

            var movimiento = await _context.MayorMovimientos.FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.Eliminado &&
                EF.Property<int>(x, "WorkshopId") == workshopId.Value);

            if (movimiento == null)
                return NotFound(new { message = "No existe el movimiento." });

            movimiento.Eliminado = true;
            movimiento.FechaEliminacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new Respuesta<object>
            {
                Ok = 1,
                Message = "Movimiento eliminado.",
                Data = { new { movimiento.Id } }
            });
        }

        private static string NormalizeCuenta(string? value, bool allowEmpty = false)
        {
            var clean = string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
            if (allowEmpty && clean == "") return "";

            var match = CuentasPermitidas.FirstOrDefault(x =>
                string.Equals(x, clean, StringComparison.OrdinalIgnoreCase));

            if (match == null)
                throw new ArgumentException("La cuenta debe ser Cliente, Proveedor o Banco.");

            return match;
        }

        private static decimal AddIva(decimal baseAmount)
        {
            return Round2(baseAmount * 1.21m);
        }

        private static decimal Round2(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static string NormalizeTipo(string? value, string cuenta)
        {
            var clean = string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
            if (clean == "")
            {
                return cuenta == "Proveedor" ? "Egreso" : "Ingreso";
            }

            var match = TiposPermitidos.FirstOrDefault(x =>
                string.Equals(x, clean, StringComparison.OrdinalIgnoreCase));

            if (match == null)
                throw new ArgumentException("El tipo debe ser Ingreso o Egreso.");

            if (cuenta == "Cliente" && match != "Ingreso")
                throw new ArgumentException("La cuenta Cliente solo admite ingresos.");

            if (cuenta == "Proveedor" && match != "Egreso")
                throw new ArgumentException("La cuenta Proveedor solo admite egresos.");

            return match;
        }
    }

    public class MayorMovimientoDto
    {
        public string? Cuenta { get; set; }
        public string? TipoMovimiento { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Referencia { get; set; }
        public string? Descripcion { get; set; }
        public decimal Importe { get; set; }
    }

    public class MayorLedgerItem
    {
        public string Id { get; set; } = "";
        public string Source { get; set; } = "";
        public int SourceId { get; set; }
        public string Cuenta { get; set; } = "";
        public string TipoMovimiento { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Referencia { get; set; } = "";
        public string? Descripcion { get; set; }
        public decimal Importe { get; set; }
        public int? BankAccountId { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankAccountIban { get; set; }
    }
}
