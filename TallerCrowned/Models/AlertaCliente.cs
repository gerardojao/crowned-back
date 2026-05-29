namespace TallerCrowned.Models
{
    public class AlertaCliente
    {
        public int Id { get; set; }

        public string Cliente { get; set; } = null!;
        public string? Telefono { get; set; }
        public string Mensaje { get; set; } = null!;

        public DateTime FechaAviso { get; set; }

        public bool Atendida { get; set; }

        public int? IdFacturaEmitida { get; set; }

        public bool Eliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}