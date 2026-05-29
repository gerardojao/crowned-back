namespace TallerCrowned.Models
{
    public class FacturaEmitida
    {
        public int Id { get; set; }

        public string NumeroFactura { get; set; } = null!;
        public int? IdOrdenTrabajo { get; set; }

        public DateTime Fecha { get; set; }

        public string Cliente { get; set; } = null!;
        public string? Dni { get; set; }
        public string? DireccionCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? Matricula { get; set; }
        public string? Km { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Otros { get; set; }
        public decimal Total { get; set; }

        public string? Observaciones { get; set; }
        public string ItemsJson { get; set; } = null!;

        public bool Eliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}