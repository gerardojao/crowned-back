namespace TallerCrowned.Models
{
    public class ServicioFrecuente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Eliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}
