using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TallerCrowned.DTOs.RepuestosStock;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RepuestoStockController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public RepuestoStockController(
            dbContext context,
            ICurrentUserService currentUserService,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? esFacturado = false,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var query = _context.RepuestosStock
                    .AsNoTracking()
                    .Include(x => x.Proveedor)
                    .Where(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();

                    query = query.Where(x =>
                        x.Nombre.ToLower().Contains(s) ||
                        (x.CodigoReferencia != null && x.CodigoReferencia.ToLower().Contains(s)) ||
                        (x.Marca != null && x.Marca.ToLower().Contains(s)) ||
                        (x.Categoria != null && x.Categoria.ToLower().Contains(s)) ||
                        (x.NumeroFactura != null && x.NumeroFactura.ToLower().Contains(s)) ||
                        (x.Cliente != null && x.Cliente.ToLower().Contains(s)) ||
                        (x.Matricula != null && x.Matricula.ToLower().Contains(s)) ||
                        (x.NombreProveedorSnapshot != null && x.NombreProveedorSnapshot.ToLower().Contains(s)) ||
                        (x.Proveedor != null && x.Proveedor.Nombre != null && x.Proveedor.Nombre.ToLower().Contains(s))
                    );
                }

                if (esFacturado.HasValue)
                    query = query.Where(x => x.EsFacturado == esFacturado.Value);

                if (fechaInicio.HasValue)
                {
                    var desde = fechaInicio.Value.Date;
                    query = query.Where(x => x.FechaFactura >= desde);
                }

                if (fechaFin.HasValue)
                {
                    var hastaExcl = fechaFin.Value.Date.AddDays(1);
                    query = query.Where(x => x.FechaFactura < hastaExcl);
                }

                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var total = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.FechaFactura)
                    .ThenByDescending(x => x.Id)
                    .ThenBy(x => x.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new RepuestoStockReadDTO
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        CodigoReferencia = x.CodigoReferencia,
                        Marca = x.Marca,
                        Categoria = x.Categoria,
                        Cantidad = x.Cantidad,
                        StockMinimo = x.StockMinimo,
                        PrecioCompra = x.PrecioCompra,
                        PrecioVenta = x.PrecioVenta,
                        Ubicacion = x.Ubicacion,
                        Observaciones = x.Observaciones,
                        IdProveedor = x.IdProveedor,
                        NombreProveedor = x.NombreProveedorSnapshot ?? (x.Proveedor != null ? x.Proveedor.Nombre : null),
                        StockBajo = !x.EsFacturado && x.Cantidad <= x.StockMinimo,
                        EsFacturado = x.EsFacturado,
                        IdFacturaEmitida = x.IdFacturaEmitida,
                        NumeroFactura = x.NumeroFactura,
                        FechaFactura = x.FechaFactura,
                        Cliente = x.Cliente,
                        Matricula = x.Matricula
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = esFacturado == true ? "Rentabilidad de repuestos facturados" : "Catalogo de repuestos";
                respuesta.Data.Add(new
                {
                    items = data,
                    total,
                    page,
                    pageSize
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

        [HttpGet("stock-bajo")]
        public async Task<ActionResult> GetStockBajo()
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var data = await _context.RepuestosStock
                    .AsNoTracking()
                    .Include(x => x.Proveedor)
                    .Where(x =>
                        !x.Eliminado &&
                        !x.EsFacturado &&
                        x.Cantidad <= x.StockMinimo &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    )
                    .OrderBy(x => x.Cantidad)
                    .ThenBy(x => x.Nombre)
                    .Select(x => new RepuestoStockReadDTO
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        CodigoReferencia = x.CodigoReferencia,
                        Marca = x.Marca,
                        Categoria = x.Categoria,
                        Cantidad = x.Cantidad,
                        StockMinimo = x.StockMinimo,
                        PrecioCompra = x.PrecioCompra,
                        PrecioVenta = x.PrecioVenta,
                        Ubicacion = x.Ubicacion,
                        Observaciones = x.Observaciones,
                        IdProveedor = x.IdProveedor,
                        NombreProveedor = x.NombreProveedorSnapshot ?? (x.Proveedor != null ? x.Proveedor.Nombre : null),
                        StockBajo = true,
                        EsFacturado = x.EsFacturado,
                        IdFacturaEmitida = x.IdFacturaEmitida,
                        NumeroFactura = x.NumeroFactura,
                        FechaFactura = x.FechaFactura,
                        Cliente = x.Cliente,
                        Matricula = x.Matricula
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Repuestos con stock bajo";
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
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var item = await _context.RepuestosStock
                    .AsNoTracking()
                    .Include(x => x.Proveedor)
                    .Where(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    )
                    .Select(x => new RepuestoStockReadDTO
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        CodigoReferencia = x.CodigoReferencia,
                        Marca = x.Marca,
                        Categoria = x.Categoria,
                        Cantidad = x.Cantidad,
                        StockMinimo = x.StockMinimo,
                        PrecioCompra = x.PrecioCompra,
                        PrecioVenta = x.PrecioVenta,
                        Ubicacion = x.Ubicacion,
                        Observaciones = x.Observaciones,
                        IdProveedor = x.IdProveedor,
                        NombreProveedor = x.NombreProveedorSnapshot ?? (x.Proveedor != null ? x.Proveedor.Nombre : null),
                        StockBajo = !x.EsFacturado && x.Cantidad <= x.StockMinimo,
                        EsFacturado = x.EsFacturado,
                        IdFacturaEmitida = x.IdFacturaEmitida,
                        NumeroFactura = x.NumeroFactura,
                        FechaFactura = x.FechaFactura,
                        Cliente = x.Cliente,
                        Matricula = x.Matricula
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el repuesto o fue eliminado.";
                    return NotFound(respuesta);
                }

                respuesta.Ok = 1;
                respuesta.Message = "Repuesto encontrado";
                respuesta.Data.Add(item);

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
        public async Task<ActionResult> Create([FromBody] RepuestoStockCreateDTO dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { message = "El nombre del repuesto es requerido." });

                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                if (dto.IdProveedor.HasValue && dto.IdProveedor.Value > 0)
                {
                    var proveedorExiste = await _context.Proveedores
                        .AnyAsync(x =>
                            x.Id == dto.IdProveedor.Value &&
                            !x.Eliminado &&
                            EF.Property<int>(x, "WorkshopId") == workshopId.Value
                        );

                    if (!proveedorExiste)
                        return BadRequest(new { message = "El proveedor indicado no existe." });
                }

                var repuesto = new RepuestoStock
                {
                    Nombre = dto.Nombre.Trim(),
                    CodigoReferencia = dto.CodigoReferencia?.Trim(),
                    Marca = dto.Marca?.Trim(),
                    Categoria = dto.Categoria?.Trim(),
                    Cantidad = dto.Cantidad,
                    StockMinimo = dto.StockMinimo <= 0 ? 3 : dto.StockMinimo,
                    PrecioCompra = dto.PrecioCompra,
                    PrecioVenta = dto.PrecioVenta,
                    Ubicacion = dto.Ubicacion?.Trim(),
                    Observaciones = dto.Observaciones?.Trim(),
                    IdProveedor = dto.IdProveedor.HasValue && dto.IdProveedor.Value > 0 ? dto.IdProveedor.Value : null,
                    EsFacturado = false,
                    Eliminado = false
                };

                _context.RepuestosStock.Add(repuesto);
                _context.Entry(repuesto).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Repuesto registrado correctamente.";
                respuesta.Data.Add(new
                {
                    repuesto.Id,
                    StockBajo = repuesto.Cantidad <= repuesto.StockMinimo
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
        public async Task<ActionResult> Update(int id, [FromBody] RepuestoStockUpdateDTO dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var repuesto = await _context.RepuestosStock
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (repuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el repuesto o fue eliminado.";
                    return NotFound(respuesta);
                }

                if (repuesto.EsFacturado)
                    return BadRequest(new { message = "No se puede editar un repuesto ya facturado." });

                if (dto.IdProveedor.HasValue)
                {
                    if (dto.IdProveedor.Value <= 0)
                    {
                        repuesto.IdProveedor = null;
                    }
                    else
                    {
                        var proveedorExiste = await _context.Proveedores
                            .AnyAsync(x =>
                                x.Id == dto.IdProveedor.Value &&
                                !x.Eliminado &&
                                EF.Property<int>(x, "WorkshopId") == workshopId.Value
                            );

                        if (!proveedorExiste)
                            return BadRequest(new { message = "El proveedor indicado no existe." });

                        repuesto.IdProveedor = dto.IdProveedor.Value;
                    }
                }

                if (!string.IsNullOrWhiteSpace(dto.Nombre)) repuesto.Nombre = dto.Nombre.Trim();

                if (dto.CodigoReferencia != null) repuesto.CodigoReferencia = dto.CodigoReferencia.Trim();
                if (dto.Marca != null) repuesto.Marca = dto.Marca.Trim();
                if (dto.Categoria != null) repuesto.Categoria = dto.Categoria.Trim();

                if (dto.Cantidad.HasValue) repuesto.Cantidad = dto.Cantidad.Value;
                if (dto.StockMinimo.HasValue) repuesto.StockMinimo = dto.StockMinimo.Value <= 0 ? 3 : dto.StockMinimo.Value;

                if (dto.PrecioCompra.HasValue) repuesto.PrecioCompra = dto.PrecioCompra.Value;
                if (dto.PrecioVenta.HasValue) repuesto.PrecioVenta = dto.PrecioVenta.Value;

                if (dto.Ubicacion != null) repuesto.Ubicacion = dto.Ubicacion.Trim();
                if (dto.Observaciones != null) repuesto.Observaciones = dto.Observaciones.Trim();

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Repuesto actualizado correctamente.";
                respuesta.Data.Add(new
                {
                    repuesto.Id,
                    StockBajo = repuesto.Cantidad <= repuesto.StockMinimo
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

        [HttpPatch("{id:int}/cantidad")]
        public async Task<ActionResult> UpdateCantidad(int id, [FromBody] int cantidad)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var repuesto = await _context.RepuestosStock
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (repuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el repuesto o fue eliminado.";
                    return NotFound(respuesta);
                }

                if (repuesto.EsFacturado)
                    return BadRequest(new { message = "No se puede cambiar la cantidad de un repuesto ya facturado." });

                repuesto.Cantidad = cantidad;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Cantidad actualizada correctamente.";
                respuesta.Data.Add(new
                {
                    repuesto.Id,
                    repuesto.Cantidad,
                    StockBajo = repuesto.Cantidad <= repuesto.StockMinimo
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
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var repuesto = await _context.RepuestosStock
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (repuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el repuesto o ya fue eliminado.";
                    return NotFound(respuesta);
                }

                if (repuesto.EsFacturado)
                    return BadRequest(new { message = "No se puede eliminar un repuesto ya facturado." });

                repuesto.Eliminado = true;
                repuesto.FechaEliminacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Repuesto eliminado correctamente.";
                respuesta.Data.Add(new
                {
                    repuesto.Id,
                    repuesto.Eliminado,
                    repuesto.FechaEliminacion
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
