namespace TallerCrowned.Models
{
    public class Workshop
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string RazonSocial { get; set; } = null!;
        public string Nif { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Iban { get; set; }
        public string SerieFactura { get; set; } = "A";
        public string? LogoPath { get; set; }
        public int MaxUsers { get; set; } = 3;
        public string? FooterText { get; set; }
        public string? PrivacyPolicyText { get; set; }
        public string? TermsText { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
