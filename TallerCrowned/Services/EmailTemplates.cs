namespace FamilyApp.Services;

public static class EmailTemplates
{
    private static string H(string? s) => System.Net.WebUtility.HtmlEncode(s ?? "");

    public static string AdminManualPayment(string email, string method, string? note) => $@"
<div style='font-family:ui-sans-serif,system-ui;'>
  <h2>Nuevo pago manual (pendiente)</h2>
  <p><b>Email:</b> {H(email)}</p>
  <p><b>Método:</b> {H(method)}</p>
  <p><b>Nota:</b> {H(note)}</p>
  <p style='color:#6b7280;font-size:12px'>Revisa el ingreso y aprueba el acceso.</p>
</div>";

    public static string UserClaimReceived(string email) => $@"
<div style='font-family:ui-sans-serif,system-ui;'>
  <h2>¡Gracias! Hemos recibido tu confirmación</h2>
  <p>Hola {H(email)},</p>
  <p>Revisaremos tu pago (transferencia/Bizum/PayPal.me) y activaremos tu acceso. Te avisaremos por email.</p>
  <p style='color:#6b7280;font-size:12px'>Tiempo habitual: 0–24 h laborables.</p>
</div>";

    public static string UserAccessActivated(string email) => $@"
<div style='font-family:ui-sans-serif,system-ui;'>
  <h2>Acceso activado</h2>
  <p>Hola {H(email)},</p>
  <p>Tu acceso a <b>FamilyApp</b> ya está activo. ¡Disfruta!</p>
  <p><a href='https://familyapp.store/login' style='background:#065f46;color:#fff;padding:10px 14px;border-radius:10px;text-decoration:none'>Entrar a FamilyApp</a></p>
</div>";
}
