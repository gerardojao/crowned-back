// Services/SmtpEmailSender.cs
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace FamilyApp.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration cfg, ILogger<SmtpEmailSender> logger)
        {
            _cfg = cfg;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, string? replyTo = null, string? fromName = null)
        {
            if (_cfg.GetValue<bool>("Email:DevPrintEmails"))
            {
                _logger.LogInformation("DEV email to {To}. Subject: {Subject}. Body: {Body}", to, subject, htmlBody);
                return;
            }

            var host = _cfg["Smtp:Host"] ?? "smtp.resend.com";
            var port = int.TryParse(_cfg["Smtp:Port"], out var p) ? p : 587;
            var user = _cfg["Smtp:User"] ?? "resend";
            var pass = _cfg["Smtp:Pass"];
            var from = _cfg["Smtp:From"] ?? _cfg["Email:From"] ?? "no-reply@tallercrowned.store";
            var display = string.IsNullOrWhiteSpace(fromName) ? "TallerCrowned" : fromName;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 55000
            };

            using var msg = new MailMessage()
            {
                From = new MailAddress(from, display, Encoding.UTF8),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Body = string.IsNullOrWhiteSpace(htmlBody) ? "<p>(sin contenido)</p>" : htmlBody
            };

            // Soportar múltiples destinatarios separados por coma/semicolon
            foreach (var addr in to.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                msg.To.Add(addr.Trim());
            }

            // Reply-To (para que al responder vaya al email del usuario)
            if (!string.IsNullOrWhiteSpace(replyTo))
                msg.ReplyToList.Add(new MailAddress(replyTo.Trim()));

            // Alternativa de texto plano (ayuda a entregabilidad)
            var plain = StripHtml(msg.Body);
            var altText = AlternateView.CreateAlternateViewFromString(plain, Encoding.UTF8, "text/plain");
            var altHtml = AlternateView.CreateAlternateViewFromString(msg.Body, Encoding.UTF8, "text/html");
            msg.AlternateViews.Add(altText);
            msg.AlternateViews.Add(altHtml);

            await client.SendMailAsync(msg);
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "(sin contenido)";
            var text = Regex.Replace(html, "<br ?/?>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "</p>", "\n\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "<.*?>", string.Empty);
            return WebUtility.HtmlDecode(text).Trim();
        }
    }
}
