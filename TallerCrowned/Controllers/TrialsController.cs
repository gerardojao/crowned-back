// Controllers/TrialsController.cs
using FamilyApp.Data;
using FamilyApp.DTOs.Trials;
using FamilyApp.Models;
using FamilyApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TrialsController : ControllerBase
{
    private readonly IEmailSender _mailer;
    private readonly IConfiguration _cfg;
    private readonly dbContext _db;
    private readonly IPasswordService _pwd;
    private readonly ITokenService _tokens;

    public TrialsController(IEmailSender mailer, IConfiguration cfg, dbContext db, IPasswordService pwd, ITokenService tokens)
    {
        _mailer = mailer; _cfg = cfg; _db = db; _pwd = pwd; _tokens = tokens;
    }

    [HttpPost("request")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestTrial([FromBody] TrialRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest(new { message = "Email requerido." });
        var email = dto.Email.Trim().ToLowerInvariant();
        if (!email.Contains("@")) return BadRequest(new { message = "Email inválido." });

        var hours = int.TryParse(_cfg["Trials:Hours"], out var h) ? h : 24;
        var expires = DateTime.UtcNow.AddHours(hours);
        var secret = _cfg["Trials:LinkSecret"] ?? throw new Exception("Trials:LinkSecret missing");
        var appBase =
            _cfg["Frontend:BaseUrl"] ??
            _cfg["App:FrontendBaseUrl"] ??
            _cfg["App:AppBaseUrl"] ??
            $"{Request.Scheme}://{Request.Host.Value}" ??
            "https://www.familyapp.store";

        var landingBase =
                _cfg["App:LandingBaseUrl"] ??
                _cfg["Frontend:LandingBaseUrl"] ??  // por si tuvieras esta
                "https://family-app-landing.vercel.app";

        // 1) token firmado
        var signed = SignedTrialToken.Create(email, expires, secret);
        var magicLink = $"{appBase}/trial?token={Uri.EscapeDataString(signed)}&email={Uri.EscapeDataString(email)}";

        // 2) emails
        var support = _cfg["Support:Email"] ?? "soporte@familyapp.store";
        await _mailer.SendAsync(support, "Nueva solicitud de prueba 24 h",
            $"<p>Email: {System.Net.WebUtility.HtmlEncode(email)}</p><p>Exp: {expires:u}</p>");

        await _mailer.SendAsync(email, "Tu acceso de prueba (24 h) – FamilyApp", $@"
            <p>Hola,</p>
            <p>Usa este enlace para entrar sin registrarte (caduca en {hours} h):</p>
            <p><a href=""{System.Net.WebUtility.HtmlEncode(magicLink)}"">Abrir FamilyApp</a></p>
            <p>Si no funciona, copia y pega:<br/>{System.Net.WebUtility.HtmlEncode(magicLink)}</p>
        ");

        return Ok(new { ok = true });
    }

    public class TrialRedeemDto { public string Email { get; set; } = ""; public string Token { get; set; } = ""; }

    [HttpPost("redeem")]
    [AllowAnonymous]
    public async Task<IActionResult> Redeem([FromBody] TrialRedeemDto dto)
    {
        var email = dto.Email?.Trim().ToLowerInvariant() ?? "";
        var token = dto.Token ?? "";
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) return BadRequest(new { message = "Datos incompletos." });

        var secret = _cfg["Trials:LinkSecret"] ?? "";
        if (!SignedTrialToken.TryValidate(token, secret, out var emailFromToken, out var expUtc))
            return BadRequest(new { message = "Token inválido o caducado." });

        if (!string.Equals(email, emailFromToken, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Email y token no coinciden." });

        // Sin tocar esquema: reusamos Users
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new AppUser
            {
                Email = email,
                FullName = "",
                PasswordHash = _pwd.Hash(Guid.NewGuid().ToString("N")), // no se usará
                Role = "trial",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        // Emitimos JWT normal y grabamos la sesión (ya tienes las columnas)
        var jti = Guid.NewGuid();
        var jwt = _tokens.CreateToken(user, jti, out var _);
        user.ActiveSessionJti = jti;
        user.ActiveSessionExpiresAt = expUtc; // el mismo vencimiento del link
        await _db.SaveChangesAsync();

        return Ok(new { token = jwt, expiresAt = expUtc, user = new { user.Id, user.Email, user.Role } });
    }
}
