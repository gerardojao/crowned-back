namespace TallerCrowned.Models
{
    public class NumeradorFactura
    {
        public int Id { get; set; }
        public int WorkshopId { get; set; } = 1;
        public string OwnerKey { get; set; } = null!;
        public string Serie { get; set; } = "A";
        public int Anio { get; set; }
        public int UltimoNumero { get; set; }
    }
}
