namespace TallerCrowned.DTOs.RepuestosStock
{
    public class RepuestoStockCreateDTO
    {
        public string Nombre { get; set; } = null!;
        public string? CodigoReferencia { get; set; }
        public string? Marca { get; set; }
        public string? Categoria { get; set; }

        public decimal Cantidad { get; set; }
        public int StockMinimo { get; set; } = 3;

        public decimal PrecioCompra { get; set; }
        public decimal? PrecioVenta { get; set; }

        public string? Ubicacion { get; set; }
        public string? Observaciones { get; set; }

        public int? IdProveedor { get; set; }
    }
}
