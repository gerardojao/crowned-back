using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerCrowned.DTOs.Proveedores;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedorController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ProveedorController(dbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] ProveedorSearchDTO filter)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var query = _context.Proveedores
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.Trim().ToLower();

                    query = query.Where(x =>
                        x.Nombre.ToLower().Contains(search) ||
                        (x.Contacto != null && x.Contacto.ToLower().Contains(search)) ||
                        (x.Telefono != null && x.Telefono.ToLower().Contains(search)) ||
                        (x.Email != null && x.Email.ToLower().Contains(search)) ||
                        (x.Categoria != null && x.Categoria.ToLower().Contains(search)) ||
                        (x.NifCif != null && x.NifCif.ToLower().Contains(search))
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.Categoria))
                {
                    var categoria = filter.Categoria.Trim().ToLower();
                    query = query.Where(x => x.Categoria != null && x.Categoria.ToLower().Contains(categoria));
                }

                var page = filter.Page <= 0 ? 1 : filter.Page;
                var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
                if (pageSize > 100) pageSize = 100;

                var total = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new Proveedor
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Contacto = x.Contacto,
                        Telefono = x.Telefono,
                        Email = x.Email,
                        Direccion = x.Direccion,
                        Categoria = x.Categoria,
                        NifCif = x.NifCif,
                        Observaciones = x.Observaciones
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Proveedores registrados";
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var proveedor = await _context.Proveedores
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    )
                    .Select(x => new Proveedor
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Contacto = x.Contacto,
                        Telefono = x.Telefono,
                        Email = x.Email,
                        Direccion = x.Direccion,
                        Categoria = x.Categoria,
                        NifCif = x.NifCif,
                        Observaciones = x.Observaciones
                    })
                    .FirstOrDefaultAsync();

                if (proveedor == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el proveedor o fue eliminado.";
                    return NotFound(respuesta);
                }

                respuesta.Ok = 1;
                respuesta.Message = "Proveedor encontrado";
                respuesta.Data.Add(proveedor);

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
        public async Task<ActionResult> Create([FromBody] ProveedorCreateDTO dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { message = "El nombre del proveedor es requerido." });

                var proveedor = new Proveedor
                {
                    Nombre = dto.Nombre.Trim(),
                    Contacto = dto.Contacto?.Trim(),
                    Telefono = dto.Telefono?.Trim(),
                    Email = dto.Email?.Trim(),
                    Direccion = dto.Direccion?.Trim(),
                    Categoria = dto.Categoria?.Trim(),
                    NifCif = dto.NifCif?.Trim(),
                    Observaciones = dto.Observaciones?.Trim(),
                    Eliminado = false
                };

                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Proveedor registrado correctamente.";
                respuesta.Data.Add(new { proveedor.Id });

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
        public async Task<ActionResult> Update(int id, [FromBody] ProveedorUpdateDTO dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var uidStr = _currentUserService.UserIdInt?.ToString() ?? "";
                var isAdmin = User.IsInRole("admin");

                var proveedor = await _context.Proveedores
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                if (proveedor == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el proveedor o fue eliminado.";
                    return NotFound(respuesta);
                }

                if (!string.IsNullOrWhiteSpace(dto.Nombre)) proveedor.Nombre = dto.Nombre.Trim();

                if (dto.Contacto != null) proveedor.Contacto = dto.Contacto.Trim();
                if (dto.Telefono != null) proveedor.Telefono = dto.Telefono.Trim();
                if (dto.Email != null) proveedor.Email = dto.Email.Trim();
                if (dto.Direccion != null) proveedor.Direccion = dto.Direccion.Trim();
                if (dto.Categoria != null) proveedor.Categoria = dto.Categoria.Trim();
                if (dto.NifCif != null) proveedor.NifCif = dto.NifCif.Trim();
                if (dto.Observaciones != null) proveedor.Observaciones = dto.Observaciones.Trim();

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Proveedor actualizado correctamente.";
                respuesta.Data.Add(new { proveedor.Id });

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

                var proveedor = await _context.Proveedores
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        (isAdmin || EF.Property<string>(x, "UsuarioCreacion") == uidStr)
                    );

                if (proveedor == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el proveedor o ya fue eliminado.";
                    return NotFound(respuesta);
                }

                proveedor.Eliminado = true;
                proveedor.FechaEliminacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Proveedor eliminado correctamente.";
                respuesta.Data.Add(new
                {
                    proveedor.Id,
                    proveedor.Eliminado,
                    proveedor.FechaEliminacion
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
