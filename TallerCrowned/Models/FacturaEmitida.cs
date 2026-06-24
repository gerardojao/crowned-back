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
        public decimal TotalFactura { get; set; }
        public decimal TotalAbonado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string TipoPago { get; set; } = "Contado";
        public string EstadoCxC { get; set; } = "Pagada";
        public int? BankAccountId { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankAccountIban { get; set; }
        public string TipoFactura { get; set; } = "Normal";
        public int? FacturaOriginalId { get; set; }
        public string? NumeroFacturaRectificada { get; set; }
        public string? MotivoRectificacion { get; set; }
        public decimal ImporteRectificado { get; set; }
        public DateTime? FechaRectificacion { get; set; }

        public string? Observaciones { get; set; }
        public string ItemsJson { get; set; } = null!;

        public bool Eliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}
