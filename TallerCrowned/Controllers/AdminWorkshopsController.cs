using FamilyApp.Data;
using FamilyApp.Models;
using FamilyApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerCrowned.Models;

namespace TallerCrowned.Controllers
{
    [Authorize(Roles = "superadmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminWorkshopsController : ControllerBase
    {
        private const int DefaultMaxUsers = 3;

        private readonly dbContext _context;
        private readonly IPasswordService _passwordService;

        public AdminWorkshopsController(dbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var workshops = await _context.Workshops
                .AsNoTracking()
                .OrderBy(x => x.Nombre)
                .Select(x => new
                {
                    x.Id,
                    x.Nombre,
                    x.RazonSocial,
                    x.Nif,
                    x.Direccion,
                    x.Email,
                    x.Telefono,
                    x.Iban,
                    x.SerieFactura,
                    x.LogoPath,
                    x.BusinessType,
                    x.TerminologyProfile,
                    x.MaxUsers,
                    x.FooterText,
                    x.PrivacyPolicyText,
                    x.TermsText,
                    x.EnableWhatsappAlerts,
                    x.EnableInvoiceExport,
                    x.EnableProfitAndLoss,
                    ActiveUsers = _context.WorkshopUsers.Count(wu => wu.WorkshopId == x.Id && wu.Activo),
                    x.Activo
                })
                .ToListAsync();

            return Ok(workshops);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AdminWorkshopCreateDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Body vacio." });
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest(new { message = "El nombre del negocio es requerido." });
            if (string.IsNullOrWhiteSpace(dto.RazonSocial)) return BadRequest(new { message = "La razon social es requerida." });
            if (string.IsNullOrWhiteSpace(dto.Nif)) return BadRequest(new { message = "El NIF/CIF es requerido." });
            if (string.IsNullOrWhiteSpace(dto.Direccion)) return BadRequest(new { message = "La direccion es requerida." });

            var nif = dto.Nif.Trim().ToUpperInvariant();
            var exists = await _context.Workshops.AnyAsync(x => x.Nif == nif);
            if (exists) return Conflict(new { message = "Ya existe un negocio con ese NIF/CIF." });

            var maxUsers = dto.MaxUsers.GetValueOrDefault(DefaultMaxUsers);
            if (maxUsers < 1 || maxUsers > DefaultMaxUsers)
                return BadRequest(new { message = $"El maximo de usuarios por negocio no puede superar {DefaultMaxUsers}." });

            var businessType = NormalizeBusinessType(dto.BusinessType);
            var terminologyProfile = NormalizeTerminologyProfile(dto.TerminologyProfile, businessType);

            await using var tx = await _context.Database.BeginTransactionAsync();

            var workshop = new Workshop
            {
                Nombre = dto.Nombre.Trim(),
                RazonSocial = dto.RazonSocial.Trim(),
                Nif = nif,
                Direccion = dto.Direccion.Trim(),
                Telefono = dto.Telefono?.Trim(),
                Email = dto.Email?.Trim(),
                Iban = dto.Iban?.Trim(),
                SerieFactura = string.IsNullOrWhiteSpace(dto.SerieFactura) ? "A" : dto.SerieFactura.Trim().ToUpperInvariant(),
                LogoPath = dto.LogoPath?.Trim(),
                BusinessType = businessType,
                TerminologyProfile = terminologyProfile,
                MaxUsers = maxUsers,
                FooterText = dto.FooterText?.Trim(),
                PrivacyPolicyText = dto.PrivacyPolicyText?.Trim(),
                TermsText = dto.TermsText?.Trim(),
                EnableWhatsappAlerts = dto.EnableWhatsappAlerts.GetValueOrDefault(true),
                EnableInvoiceExport = dto.EnableInvoiceExport.GetValueOrDefault(true),
                EnableProfitAndLoss = dto.EnableProfitAndLoss.GetValueOrDefault(true),
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Workshops.Add(workshop);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(dto.OwnerEmail))
            {
                var user = await GetOrCreateUser(dto.OwnerEmail, dto.OwnerPassword, dto.OwnerFullName);
                if (user == null)
                    return BadRequest(new { message = "La contraseña es requerida para crear un usuario nuevo." });

                _context.WorkshopUsers.Add(new WorkshopUser
                {
                    WorkshopId = workshop.Id,
                    UserId = user.Id,
                    Role = "owner",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            await tx.CommitAsync();

            return Ok(new { workshop.Id, workshop.Nombre, workshop.Nif, workshop.BusinessType, workshop.TerminologyProfile, workshop.MaxUsers });
        }

        [HttpPost("{workshopId:int}/users")]
        public async Task<ActionResult> AddUser(int workshopId, [FromBody] AdminWorkshopUserCreateDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Body vacio." });
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest(new { message = "El email es requerido." });

            var workshop = await _context.Workshops.FirstOrDefaultAsync(x => x.Id == workshopId && x.Activo);
            if (workshop == null) return NotFound(new { message = "No existe el negocio." });

            var activeUsers = await _context.WorkshopUsers.CountAsync(x => x.WorkshopId == workshopId && x.Activo);
            var maxUsers = workshop.MaxUsers <= 0 ? DefaultMaxUsers : workshop.MaxUsers;
            if (activeUsers >= maxUsers)
                return BadRequest(new { message = $"Este negocio ya tiene el maximo permitido de {maxUsers} usuarios activos." });

            var user = await GetOrCreateUser(dto.Email, dto.Password, dto.FullName);
            if (user == null)
                return BadRequest(new { message = "La contraseña es requerida para crear un usuario nuevo." });

            var relation = await _context.WorkshopUsers.FirstOrDefaultAsync(x => x.WorkshopId == workshopId && x.UserId == user.Id);
            if (relation != null)
            {
                if (relation.Activo) return Conflict(new { message = "El usuario ya pertenece a este negocio." });

                relation.Activo = true;
                relation.Role = NormalizeWorkshopRole(dto.Role);
            }
            else
            {
                _context.WorkshopUsers.Add(new WorkshopUser
                {
                    WorkshopId = workshopId,
                    UserId = user.Id,
                    Role = NormalizeWorkshopRole(dto.Role),
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { user.Id, user.Email, WorkshopId = workshopId });
        }

        [HttpGet("{workshopId:int}/users")]
        public async Task<ActionResult> GetUsers(int workshopId)
        {
            var workshopExists = await _context.Workshops.AnyAsync(x => x.Id == workshopId);
            if (!workshopExists) return NotFound(new { message = "No existe el negocio." });

            var users = await _context.WorkshopUsers
                .AsNoTracking()
                .Where(x => x.WorkshopId == workshopId)
                .Join(
                    _context.Users.AsNoTracking(),
                    wu => wu.UserId,
                    u => u.Id,
                    (wu, u) => new
                    {
                        WorkshopUserId = wu.Id,
                        wu.WorkshopId,
                        wu.UserId,
                        u.Email,
                        u.FullName,
                        SystemRole = u.Role,
                        WorkshopRole = wu.Role,
                        UserActive = u.IsActive,
                        WorkshopUserActive = wu.Activo,
                        wu.FechaCreacion
                    })
                .OrderByDescending(x => x.WorkshopUserActive)
                .ThenBy(x => x.Email)
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("{workshopId:int}/users/{userId:int}")]
        public async Task<ActionResult> UpdateUser(int workshopId, int userId, [FromBody] AdminWorkshopUserUpdateDto dto)
        {
            var relation = await _context.WorkshopUsers
                .FirstOrDefaultAsync(x => x.WorkshopId == workshopId && x.UserId == userId);
            if (relation == null) return NotFound(new { message = "El usuario no pertenece a este negocio." });

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return NotFound(new { message = "No existe el usuario." });

            if (dto.WorkshopRole != null)
                relation.Role = NormalizeWorkshopRole(dto.WorkshopRole);

            if (dto.FullName != null)
                user.FullName = string.IsNullOrWhiteSpace(dto.FullName) ? null : dto.FullName.Trim();

            if (dto.SystemRole != null)
                user.Role = NormalizeSystemRole(dto.SystemRole, user.Role);

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            if (dto.WorkshopUserActive.HasValue && dto.WorkshopUserActive.Value != relation.Activo)
            {
                if (dto.WorkshopUserActive.Value)
                {
                    var workshop = await _context.Workshops.AsNoTracking().FirstOrDefaultAsync(x => x.Id == workshopId);
                    if (workshop == null) return NotFound(new { message = "No existe el negocio." });

                    var activeUsers = await _context.WorkshopUsers.CountAsync(x => x.WorkshopId == workshopId && x.Activo);
                    var maxUsers = workshop.MaxUsers <= 0 ? DefaultMaxUsers : workshop.MaxUsers;
                    if (activeUsers >= maxUsers)
                        return BadRequest(new { message = $"Este negocio ya tiene el maximo permitido de {maxUsers} usuarios activos." });
                }

                relation.Activo = dto.WorkshopUserActive.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = _passwordService.Hash(dto.Password);
                user.ActiveSessionJti = null;
                user.ActiveSessionExpiresAt = null;
            }

            await _context.SaveChangesAsync();
            return Ok(new
            {
                user.Id,
                user.Email,
                user.FullName,
                SystemRole = user.Role,
                WorkshopRole = relation.Role,
                UserActive = user.IsActive,
                WorkshopUserActive = relation.Activo
            });
        }

        [HttpDelete("{workshopId:int}/users/{userId:int}")]
        public async Task<ActionResult> RemoveUser(int workshopId, int userId)
        {
            var relation = await _context.WorkshopUsers
                .FirstOrDefaultAsync(x => x.WorkshopId == workshopId && x.UserId == userId);
            if (relation == null) return NotFound(new { message = "El usuario no pertenece a este negocio." });

            if (!relation.Activo)
                return Ok(new { relation.UserId, relation.WorkshopId, relation.Activo });

            relation.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { relation.UserId, relation.WorkshopId, relation.Activo });
        }

        [HttpPut("{workshopId:int}/legal")]
        public async Task<ActionResult> UpdateLegal(int workshopId, [FromBody] AdminWorkshopLegalDto dto)
        {
            var workshop = await _context.Workshops.FirstOrDefaultAsync(x => x.Id == workshopId);
            if (workshop == null) return NotFound(new { message = "No existe el negocio." });

            if (dto.MaxUsers.HasValue)
            {
                if (dto.MaxUsers.Value < 1 || dto.MaxUsers.Value > DefaultMaxUsers)
                    return BadRequest(new { message = $"El maximo de usuarios por negocio no puede superar {DefaultMaxUsers}." });

                var activeUsers = await _context.WorkshopUsers.CountAsync(x => x.WorkshopId == workshopId && x.Activo);
                if (activeUsers > dto.MaxUsers.Value)
                    return BadRequest(new { message = "No puedes bajar el limite por debajo de los usuarios activos actuales." });

                workshop.MaxUsers = dto.MaxUsers.Value;
            }

            workshop.FooterText = dto.FooterText?.Trim();
            workshop.PrivacyPolicyText = dto.PrivacyPolicyText?.Trim();
            workshop.TermsText = dto.TermsText?.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                workshop.Nombre = dto.Nombre.Trim();
            if (!string.IsNullOrWhiteSpace(dto.RazonSocial))
                workshop.RazonSocial = dto.RazonSocial.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Nif))
            {
                var nif = dto.Nif.Trim().ToUpperInvariant();
                var exists = await _context.Workshops.AnyAsync(x => x.Id != workshopId && x.Nif == nif);
                if (exists) return Conflict(new { message = "Ya existe otro negocio con ese NIF/CIF." });
                workshop.Nif = nif;
            }
            if (!string.IsNullOrWhiteSpace(dto.Direccion))
                workshop.Direccion = dto.Direccion.Trim();
            if (dto.Telefono != null)
                workshop.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            if (dto.Email != null)
                workshop.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            if (dto.Iban != null)
                workshop.Iban = string.IsNullOrWhiteSpace(dto.Iban) ? null : dto.Iban.Trim();
            if (dto.SerieFactura != null)
                workshop.SerieFactura = string.IsNullOrWhiteSpace(dto.SerieFactura) ? "A" : dto.SerieFactura.Trim().ToUpperInvariant();
            if (dto.LogoPath != null)
                workshop.LogoPath = string.IsNullOrWhiteSpace(dto.LogoPath) ? null : dto.LogoPath.Trim();
            if (dto.Activo.HasValue)
                workshop.Activo = dto.Activo.Value;
            if (dto.EnableWhatsappAlerts.HasValue)
                workshop.EnableWhatsappAlerts = dto.EnableWhatsappAlerts.Value;
            if (dto.EnableInvoiceExport.HasValue)
                workshop.EnableInvoiceExport = dto.EnableInvoiceExport.Value;
            if (dto.EnableProfitAndLoss.HasValue)
                workshop.EnableProfitAndLoss = dto.EnableProfitAndLoss.Value;
            if (dto.BusinessType != null)
                workshop.BusinessType = NormalizeBusinessType(dto.BusinessType);
            if (dto.TerminologyProfile != null)
                workshop.TerminologyProfile = NormalizeTerminologyProfile(dto.TerminologyProfile, workshop.BusinessType);

            await _context.SaveChangesAsync();
            return Ok(new { workshop.Id, workshop.BusinessType, workshop.TerminologyProfile, workshop.MaxUsers });
        }

        private async Task<AppUser?> GetOrCreateUser(string emailRaw, string? password, string? fullName)
        {
            var email = emailRaw.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user != null) return user;

            if (string.IsNullOrWhiteSpace(password))
                return null;

            user = new AppUser
            {
                Email = email,
                FullName = fullName?.Trim(),
                PasswordHash = _passwordService.Hash(password),
                Role = "user",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private static string NormalizeWorkshopRole(string? role)
        {
            var clean = string.IsNullOrWhiteSpace(role) ? "user" : role.Trim().ToLowerInvariant();
            return clean is "owner" or "manager" or "mechanic" or "viewer" or "user" ? clean : "user";
        }

        private static string NormalizeSystemRole(string role, string currentRole)
        {
            var clean = role.Trim().ToLowerInvariant();
            if (currentRole == "superadmin" && clean != "superadmin") return currentRole;
            return clean is "superadmin" or "admin" or "user" ? clean : "user";
        }

        private static string NormalizeBusinessType(string? type)
        {
            var clean = string.IsNullOrWhiteSpace(type) ? "automotive" : type.Trim().ToLowerInvariant();
            return clean is "automotive" or "technical_services" or "generic_services" or "invoice_only"
                ? clean
                : "automotive";
        }

        private static string NormalizeTerminologyProfile(string? profile, string businessType)
        {
            var clean = string.IsNullOrWhiteSpace(profile) ? "" : profile.Trim().ToLowerInvariant();
            if (clean is "automotive" or "equipment_service" or "generic_service")
                return clean;

            return businessType is "technical_services" or "generic_services"
                ? "equipment_service"
                : "automotive";
        }
    }

    public class AdminWorkshopCreateDto
    {
        public string Nombre { get; set; } = "";
        public string RazonSocial { get; set; } = "";
        public string Nif { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Iban { get; set; }
        public string? SerieFactura { get; set; }
        public string? LogoPath { get; set; }
        public string? BusinessType { get; set; }
        public string? TerminologyProfile { get; set; }
        public int? MaxUsers { get; set; }
        public string? FooterText { get; set; }
        public string? PrivacyPolicyText { get; set; }
        public string? TermsText { get; set; }
        public bool? EnableWhatsappAlerts { get; set; }
        public bool? EnableInvoiceExport { get; set; }
        public bool? EnableProfitAndLoss { get; set; }
        public string? OwnerEmail { get; set; }
        public string? OwnerPassword { get; set; }
        public string? OwnerFullName { get; set; }
    }

    public class AdminWorkshopUserCreateDto
    {
        public string Email { get; set; } = "";
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }

    public class AdminWorkshopUserUpdateDto
    {
        public string? FullName { get; set; }
        public string? WorkshopRole { get; set; }
        public string? SystemRole { get; set; }
        public bool? IsActive { get; set; }
        public bool? WorkshopUserActive { get; set; }
        public string? Password { get; set; }
    }

    public class AdminWorkshopLegalDto
    {
        public string? Nombre { get; set; }
        public string? RazonSocial { get; set; }
        public string? Nif { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Iban { get; set; }
        public string? SerieFactura { get; set; }
        public string? LogoPath { get; set; }
        public bool? Activo { get; set; }
        public int? MaxUsers { get; set; }
        public string? BusinessType { get; set; }
        public string? TerminologyProfile { get; set; }
        public string? FooterText { get; set; }
        public string? PrivacyPolicyText { get; set; }
        public string? TermsText { get; set; }
        public bool? EnableWhatsappAlerts { get; set; }
        public bool? EnableInvoiceExport { get; set; }
        public bool? EnableProfitAndLoss { get; set; }
    }
}
