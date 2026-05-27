namespace TallerCrowned.DTOs.RepuestosStock
{
    public class RepuestoStockReadDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;
        public string? CodigoReferencia { get; set; }
        public string? Marca { get; set; }
        public string? Categoria { get; set; }

        public int Cantidad { get; set; }
        public int StockMinimo { get; set; }

        public decimal PrecioCompra { get; set; }
        public decimal? PrecioVenta { get; set; }

        public string? Ubicacion { get; set; }
        public string? Observaciones { get; set; }

        public int IdProveedor { get; set; }
        public string? NombreProveedor { get; set; }

        public bool StockBajo { get; set; }
    }
}