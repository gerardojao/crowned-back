namespace TallerCrowned.DTOs.Proveedores
{
    public class ProveedorCreateDTO
    {
        public string Nombre { get; set; } = null!;
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }

        public string? Categoria { get; set; }
        public string? NifCif { get; set; }
        public string? Observaciones { get; set; }
    }
}


