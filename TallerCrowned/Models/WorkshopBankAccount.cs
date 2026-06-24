namespace TallerCrowned.Models
{
    public class WorkshopBankAccount
    {
        public int Id { get; set; }
        public int WorkshopId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Iban { get; set; } = null!;
        public bool EsPrincipal { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public Workshop? Workshop { get; set; }
    }
}
