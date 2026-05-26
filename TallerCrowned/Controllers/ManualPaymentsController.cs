using FamilyApp.Data;
using FamilyApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/manual-payments")]
public class ManualPaymentsController : ControllerBase
{
    private readonly IEmailSender _mailer;
    private readonly dbContext _db;

    public ManualPaymentsController(IEmailSender mailer, dbContext db)
    {
        _mailer = mailer;
        _db = db;
    }

    public class ClaimDto
    {
        public string Email { get; set; } = "";
        public string Method { get; set; } = ""; // "Transferencia" | "Bizum" | "PayPal.me"
        public string? Note { get; set; }        // nº operación / comentario
    }

    // Cliente declara "He pagado"
    [HttpPost("claim")]
    public async Task<IActionResult> CreateClaim([FromBody] ClaimDto dto)
    {
        var email = (dto.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email requerido." });

        // 1) Aviso interno (ventas)
        await _mailer.SendAsync(
            "soporte@familyapp.store",//gerardojao
            "Pago manual recibido",
            EmailTemplates.AdminManualPayment("soporte@familyapp.store", dto.Method, dto.Note)
        );

        // 2) Acuse de recibo al cliente
        await _mailer.SendAsync(
            email,
            "Hemos recibido tu confirmación de pago",
            EmailTemplates.UserClaimReceived(email)
        );

        return Ok(new { ok = true });
    }

    public class ApproveDto { public string Email { get; set; } = ""; }

    // Activación manual por email (llámalo tras verificar el ingreso)
    [HttpPost("approve")]
    public async Task<IActionResult> Approve([FromBody] ApproveDto dto)
    {
        var email = (dto.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email requerido." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound(new { message = "Usuario no encontrado." });

        user.IsActive = true;
        await _db.SaveChangesAsync();

        await _mailer.SendAsync(
            email,
            "Acceso activado — FamilyApp",
            EmailTemplates.UserAccessActivated(email)
        );

        return Ok(new { ok = true });
    }
}
