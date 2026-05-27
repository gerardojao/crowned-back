namespace TallerCrowned.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }

        public string? Categoria { get; set; }
        public string? NifCif { get; set; }
        public string? Observaciones { get; set; }

        public bool Eliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}
