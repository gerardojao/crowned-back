namespace TallerCrowned.DTOs.RepuestosStock
{
    public class RepuestoStockRentabilidadUpdateDTO
    {
        public string? Nombre { get; set; }
        public decimal? Cantidad { get; set; }
        public decimal? PrecioCompra { get; set; }
        public int? IdProveedor { get; set; }
    }
}
