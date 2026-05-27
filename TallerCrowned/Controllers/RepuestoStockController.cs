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

        public RepuestoStockController(dbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var query = _context.RepuestosStock
                    .AsNoTracking()
                    .Include(x => x.Proveedor)
                    .Where(x =>
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();

                    query = query.Where(x =>
                        x.Nombre.ToLower().Contains(s) ||
                        (x.CodigoReferencia != null && x.CodigoReferencia.ToLower().Contains(s)) ||
                        (x.Marca != null && x.Marca.ToLower().Contains(s)) ||
                        (x.Categoria != null && x.Categoria.ToLower().Contains(s)) ||
                        (x.Proveedor.Nombre != null && x.Proveedor.Nombre.ToLower().Contains(s))
                    );
                }

                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var total = await query.CountAsync();

                var data = await query
                    .OrderBy(x => x.Cantidad <= x.StockMinimo ? 0 : 1)
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
                        NombreProveedor = x.Proveedor.Nombre,
                        StockBajo = x.Cantidad <= x.StockMinimo
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Stock de repuestos";
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
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var data = await _context.RepuestosStock
                    .AsNoTracking()
                    .Include(x => x.Proveedor)
                    .Where(x =>
                        !x.Eliminado &&
                        x.Cantidad <= x.StockMinimo &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
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
                        NombreProveedor = x.Proveedor.Nombre,
                        StockBajo = true
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
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var item = await _context.RepuestosStock
                    .AsNoTracking()
                    .Include(x => x.Proveedor)
                    .Where(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
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
                        NombreProveedor = x.Proveedor.Nombre,
                        StockBajo = x.Cantidad <= x.StockMinimo
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

                if (dto.IdProveedor <= 0)
                    return BadRequest(new { message = "El proveedor es requerido." });

                var proveedorExiste = await _context.Proveedores
                    .AnyAsync(x => x.Id == dto.IdProveedor && !x.Eliminado);

                if (!proveedorExiste)
                    return BadRequest(new { message = "El proveedor indicado no existe." });

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
                    IdProveedor = dto.IdProveedor,
                    Eliminado = false
                };

                _context.RepuestosStock.Add(repuesto);
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
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var repuesto = await _context.RepuestosStock
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                if (repuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el repuesto o fue eliminado.";
                    return NotFound(respuesta);
                }

                if (dto.IdProveedor.HasValue)
                {
                    var proveedorExiste = await _context.Proveedores
                        .AnyAsync(x => x.Id == dto.IdProveedor.Value && !x.Eliminado);

                    if (!proveedorExiste)
                        return BadRequest(new { message = "El proveedor indicado no existe." });

                    repuesto.IdProveedor = dto.IdProveedor.Value;
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
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var repuesto = await _context.RepuestosStock
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                if (repuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el repuesto o fue eliminado.";
                    return NotFound(respuesta);
                }

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
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var repuesto = await _context.RepuestosStock
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                if (repuesto == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el repuesto o ya fue eliminado.";
                    return NotFound(respuesta);
                }

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