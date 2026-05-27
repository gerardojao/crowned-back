namespace TallerCrowned.DTOs.Proveedores
{
    public class ProveedorSearchDTO
    {
        public string? Search { get; set; }
        public string? Categoria { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
