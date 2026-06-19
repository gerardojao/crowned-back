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
        public string BusinessType { get; set; } = "automotive";
        public string TerminologyProfile { get; set; } = "automotive";
        public int MaxUsers { get; set; } = 3;
        public string? FooterText { get; set; }
        public string? PrivacyPolicyText { get; set; }
        public string? TermsText { get; set; }
        public bool EnableWhatsappAlerts { get; set; } = true;
        public bool EnableInvoiceExport { get; set; } = true;
        public bool EnableProfitAndLoss { get; set; } = true;
        public bool EnableDashboardRepairVehicles { get; set; } = true;
        public bool EnableAccountsReceivable { get; set; } = true;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
