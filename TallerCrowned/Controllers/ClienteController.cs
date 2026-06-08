using FamilyApp.Data;
using FamilyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerCrowned.DTOs.Cliente;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentWorkshopService _currentWorkshopService;

        public ClienteController(
            dbContext context,
            ICurrentUserService currentUserService,
            ICurrentWorkshopService currentWorkshopService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _currentWorkshopService = currentWorkshopService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] ClienteSearchDTO filter)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var query = _context.Clientes
                    .AsNoTracking()
                    .Where(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.Trim().ToLower();

                    query = query.Where(x =>
                        x.Nombre.ToLower().Contains(search) ||
                        x.Telefono.ToLower().Contains(search) ||
                        x.Matricula.ToLower().Contains(search) ||
                        x.Modelo.ToLower().Contains(search) ||
                        (x.Email != null && x.Email.ToLower().Contains(search))
                    );
                }

                if (!string.IsNullOrWhiteSpace(filter.Matricula))
                {
                    var matricula = filter.Matricula.Trim().ToLower();
                    query = query.Where(x => x.Matricula.ToLower().Contains(matricula));
                }

                if (!string.IsNullOrWhiteSpace(filter.Telefono))
                {
                    var telefono = filter.Telefono.Trim().ToLower();
                    query = query.Where(x => x.Telefono.ToLower().Contains(telefono));
                }

                var page = filter.Page <= 0 ? 1 : filter.Page;
                var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
                if (pageSize > 100) pageSize = 100;

                var total = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new Cliente
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Dni = x.Dni,
                        Telefono = x.Telefono,
                        Email = x.Email,
                        Direccion = x.Direccion,
                        Matricula = x.Matricula,
                        Marca = x.Marca,
                        Modelo = x.Modelo,
                       
                        Kilometraje = x.Kilometraje,
                        Observaciones = x.Observaciones
                    })
                    .ToListAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Clientes registrados";
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
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var cliente = await _context.Clientes
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    )
                    .Select(x => new Cliente
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Dni = x.Dni,
                        Telefono = x.Telefono,
                        Email = x.Email,
                        Direccion = x.Direccion,
                        Matricula = x.Matricula,
                        Marca = x.Marca,
                        Modelo = x.Modelo,
                  
                        Kilometraje = x.Kilometraje,
                        Observaciones = x.Observaciones
                    })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el cliente o fue eliminado.";
                    return NotFound(respuesta);
                }

                respuesta.Ok = 1;
                respuesta.Message = "Cliente encontrado";
                respuesta.Data.Add(cliente);

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
        public async Task<ActionResult> Create([FromBody] ClienteCreateDTO dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { message = "El nombre es requerido." });

                if (string.IsNullOrWhiteSpace(dto.Telefono))
                    return BadRequest(new { message = "El teléfono es requerido." });

                if (string.IsNullOrWhiteSpace(dto.Matricula))
                    return BadRequest(new { message = "La matrícula es requerida." });

                if (string.IsNullOrWhiteSpace(dto.Modelo))
                    return BadRequest(new { message = "El modelo es requerido." });

                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var matricula = dto.Matricula.Trim().ToUpper();
                var existing = await _context.Clientes
                    .AsNoTracking()
                    .AnyAsync(x =>
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value &&
                        x.Matricula == matricula);

                if (existing)
                    return BadRequest(new { message = "Cliente ya registrado." });

                var cliente = new Cliente
                {
                    Nombre = dto.Nombre.Trim(),
                    Dni = dto.Dni?.Trim(),
                    Telefono = dto.Telefono.Trim(),
                    Email = dto.Email?.Trim(),
                    Direccion = dto.Direccion?.Trim(),
                    Matricula = matricula,
                    Marca = dto.Marca?.Trim(),
                    Modelo = dto.Modelo.Trim(),
              
                    Kilometraje = dto.Kilometraje,
                    Observaciones = dto.Observaciones?.Trim(),
                    Eliminado = false
                };

                _context.Clientes.Add(cliente);
                _context.Entry(cliente).Property("WorkshopId").CurrentValue = workshopId.Value;
                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Cliente registrado correctamente.";
                respuesta.Data.Add(new { cliente.Id });

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
        public async Task<ActionResult> Update(int id, [FromBody] ClienteUpdateDTO dto)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                var workshopId = await _currentWorkshopService.GetCurrentWorkshopIdAsync();
                if (!workshopId.HasValue) return Forbid();

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (cliente == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el cliente o fue eliminado.";
                    return NotFound(respuesta);
                }

                if (!string.IsNullOrWhiteSpace(dto.Nombre)) cliente.Nombre = dto.Nombre.Trim();
                if (dto.Dni != null) cliente.Dni = dto.Dni.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Telefono)) cliente.Telefono = dto.Telefono.Trim();

                if (dto.Email != null) cliente.Email = dto.Email.Trim();
                if (dto.Direccion != null) cliente.Direccion = dto.Direccion.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Matricula)) cliente.Matricula = dto.Matricula.Trim().ToUpper();
                if (dto.Marca != null) cliente.Marca = dto.Marca.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Modelo)) cliente.Modelo = dto.Modelo.Trim();

                
                if (dto.Kilometraje.HasValue) cliente.Kilometraje = dto.Kilometraje.Value;

                if (dto.Observaciones != null) cliente.Observaciones = dto.Observaciones.Trim();

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Cliente actualizado correctamente.";
                respuesta.Data.Add(new { cliente.Id });

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

                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.Eliminado &&
                        EF.Property<int>(x, "WorkshopId") == workshopId.Value
                    );

                if (cliente == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el cliente o ya fue eliminado.";
                    return NotFound(respuesta);
                }

                cliente.Eliminado = true;
                cliente.FechaEliminacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Cliente eliminado correctamente.";
                respuesta.Data.Add(new
                {
                    cliente.Id,
                    cliente.Eliminado,
                    cliente.FechaEliminacion
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
