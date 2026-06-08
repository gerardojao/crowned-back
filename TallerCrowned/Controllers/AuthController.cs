// Controllers/AuthController.cs
using FamilyApp.Data;
using FamilyApp.DTOs.Auth;
using FamilyApp.DTOs.LoginDTO;
using FamilyApp.Models;
using FamilyApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string AuthCookieName = ".zagapro.auth";
    private static readonly string[] LegacyAuthCookieNames = [".tallercrowned.auth", ".familyapp.auth"];

    private readonly dbContext _db;
    private readonly IPasswordService _pwd;
    private readonly ITokenService _tokens;
    private readonly IEmailSender _mailer;
    private readonly IConfiguration _cfg;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        dbContext db,
        IPasswordService pwd,
        ITokenService tokens,
        IEmailSender mailer,
        IConfiguration cfg,
        IWebHostEnvironment env,
        ILogger<AuthController> logger)
    {
        _db = db;
        _pwd = pwd;
        _tokens = tokens;
        _mailer = mailer;
        _cfg = cfg;
        _env = env;
        _logger = logger;
    }

    public class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

        [Authorize]
        [HttpGet("auth/whoami")]
        public IActionResult WhoAmI() => Ok(new
        {
            sub = User.FindFirst("sub")?.Value,
            email = User.FindFirst("email")?.Value,
            jti = User.FindFirst("jti")?.Value
        });


    //[HttpPost("login")]
    //public async Task<IActionResult> Login([FromBody] LoginDto dto)
    //{
    //    var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

    //    // Usuario no existe o password incorrecto → 401 genérico
    //    if (user == null || !_pwd.Verify(user.PasswordHash, dto.Password))
    //        return Unauthorized(new { message = "Credenciales inválidas" });

    //    // Usuario inactivo → 403 con mensaje específico
    //    if (!user.IsActive)
    //        return StatusCode(StatusCodes.Status403Forbidden, new
    //        {
    //            code = "inactive",
    //            message = "Tu cuenta aún no está activa. Revisa tu correo o contacta soporte."
    //        });

    //    var jti = Guid.NewGuid();
    //    var token = _tokens.CreateToken(user, jti, out var expiresAt);

    //    user.ActiveSessionJti = jti;
    //    user.ActiveSessionExpiresAt = expiresAt;
    //    await _db.SaveChangesAsync();
    //    var isProd = Request.Host.Host.EndsWith("familyapp.store", StringComparison.OrdinalIgnoreCase);
    //    Response.Cookies.Append(".familyapp.auth", token, new CookieOptions
    //    {
    //        Domain = isProd ? ".familyapp.store" : null,
    //        Path = "/",
    //        HttpOnly = true,
    //        Secure = true,
    //        SameSite = SameSiteMode.None,
    //        Expires = expiresAt
    //    });

    //    return Ok(new { token, expiresAt, user = new { user.Id, user.Email, user.Role } });
    //}

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !_pwd.Verify(user.PasswordHash, dto.Password))
            return Unauthorized(new { message = "Credenciales inválidas" });

        if (!user.IsActive)
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                code = "inactive",
                message = "Tu cuenta aún no está activa. Revisa tu correo o contacta soporte."
            });

        var jti = Guid.NewGuid();
        var token = _tokens.CreateToken(user, jti, out var expiresAt);
        user.ActiveSessionJti = jti;
        user.ActiveSessionExpiresAt = expiresAt;
        await _db.SaveChangesAsync();

        var isProd = _env.IsProduction();
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt
        };
        if (_env.IsDevelopment())
        {
            cookieOptions.SameSite = SameSiteMode.Lax; // evita bloqueo 3P
            cookieOptions.Secure = false;            // permite http en local
        }
        else
        {
            cookieOptions.SameSite = SameSiteMode.None;
            cookieOptions.Secure = true;
            cookieOptions.Domain = GetAuthCookieDomain();
        }

        Response.Cookies.Append(AuthCookieName, token, cookieOptions);

        return Ok(new { token, expiresAt, user = new { user.Id, user.Email, user.Role } });
    }

    
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Opcional: si hay auth y quieres limpiar sesión única en DB
        if (User?.Identity?.IsAuthenticated == true)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (int.TryParse(userIdStr, out var userId))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    user.ActiveSessionJti = null;
                    user.ActiveSessionExpiresAt = null;
                    await _db.SaveChangesAsync();
                }
            }
        }

        var isProd = _env.IsProduction();
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        };
        if (isProd) cookieOptions.Domain = GetAuthCookieDomain();

        Response.Cookies.Delete(AuthCookieName, cookieOptions);
        foreach (var legacyCookieName in LegacyAuthCookieNames)
        {
            Response.Cookies.Delete(legacyCookieName, cookieOptions);
        }
        return Ok(new { message = "Sesión cerrada." });
    }

    private string? GetAuthCookieDomain()
    {
        var host = Request.Host.Host;
        return host.EndsWith("zagapro.store", StringComparison.OrdinalIgnoreCase)
            ? ".zagapro.store"
            : null;
    }


    // POST: api/auth/register
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDTO dto)
        => StatusCode(StatusCodes.Status403Forbidden, new
        {
            ok = false,
            message = "El registro publico esta deshabilitado. Contacta con el administrador del sistema para crear usuarios de taller."
        });

    // === FORGOT PASSWORD ===
    [AllowAnonymous]
    [HttpPost("forgot")]
    public async Task<IActionResult> Forgot([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return Ok(new { message = "Si el email existe, se ha enviado un enlace de reseteo." });

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user == null)
            return Ok(new { message = "Si el email existe, se ha enviado un enlace de reseteo." });

        // 1) token y hash
        var token = SecureToken.CreateUrlToken(32);
        var tokenHash = SecureToken.Sha256Base64(token);
        var now = DateTime.UtcNow;

        await _db.PasswordResets
            .Where(x => x.UserId == user.Id && x.UsedAt == null && x.ExpiresAt >= now)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UsedAt, now));

        _db.PasswordResets.Add(new PasswordReset
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.AddMinutes(30),
            RequestIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            RequestUserAgent = Request.Headers.UserAgent.ToString()
        });
        await _db.SaveChangesAsync();

        // 2) URL del front desde config (usa SIEMPRE config, nunca Request.Host)
        //    NOTA: define App:FrontendBaseUrl = https://www.tallercrowned.store
        var frontBase = _cfg["App:FrontendBaseUrl"] ?? _cfg["App:AppBaseUrl"] ?? "https://www.tallercrowned.store";
        frontBase = frontBase.TrimEnd('/');
        var link = $"{frontBase}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

        // 3) Nombre visible (fallback al email) + encode seguro
        var display = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
        string H(string s) => System.Net.WebUtility.HtmlEncode(s);

        var html = $@"
        <p>Hola {H(display)},</p>
        <p>Has solicitado restablecer tu contraseña. Haz clic en el siguiente enlace:</p>
        <p><a href=""{H(link)}"">Restablecer contraseña</a></p>
        <p>Si no funciona, copia y pega esta URL en tu navegador:<br/>{H(link)}</p>
        <p>El enlace caduca en 30 minutos. Si no lo solicitaste, ignora este correo.</p>";

        try
        {
            await _mailer.SendAsync(user.Email, "Restablecer contraseña - ZagaPro", html);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar el correo de recuperacion para el usuario {UserId}.", user.Id);
            if (_env.IsDevelopment())
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "No se pudo enviar el correo de recuperacion. Revisa la configuracion SMTP." });

            return Ok(new { message = "Si el email existe, se ha enviado un enlace de reseteo." });
        }

        if (_env.IsDevelopment())
            return Ok(new { message = "Email enviado (DEV).", devToken = token, devLink = link });

        return Ok(new { message = "Si el email existe, se ha enviado un enlace de reseteo." });
    }


    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Vary"] = "Cookie";

        return Ok(new
        {
            sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            email = User.FindFirst("email")?.Value,
            name = User.Identity?.Name,
            roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray(),
            jti = User.FindFirst("jti")?.Value
        });
    }



    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "Datos incompletos." });

        // Reglas de contraseña (ajusta a tus políticas)
        if (dto.NewPassword.Length < 8 ||
            !dto.NewPassword.Any(char.IsUpper) ||
            !dto.NewPassword.Any(char.IsLower) ||
            !dto.NewPassword.Any(char.IsDigit))
        {
            return BadRequest(new { message = "La nueva contraseña no cumple los requisitos." });
        }

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user == null) return BadRequest(new { message = "Token inválido o caducado." });

        var tokenHash = SecureToken.Sha256Base64(dto.Token);
        var pr = await _db.PasswordResets
            .Where(x => x.UserId == user.Id && x.TokenHash == tokenHash)
            .FirstOrDefaultAsync();

        if (pr == null || pr.UsedAt != null || pr.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new { message = "Token inválido o caducado." });

        // Actualizamos contraseña
        user.PasswordHash = _pwd.Hash(dto.NewPassword);

        // Forzamos cierre de sesión de todos los dispositivos
        user.ActiveSessionJti = null;
        user.ActiveSessionExpiresAt = null;

        var now = DateTime.UtcNow;

        await _db.PasswordResets
            .Where(x => x.UserId == user.Id && x.UsedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UsedAt, now));

        pr.UsedAt = now;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Contraseña actualizada. Vuelve a iniciar sesión." });
    }
}
