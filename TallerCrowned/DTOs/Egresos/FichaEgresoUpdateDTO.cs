namespace FamilyApp.DTOs.Egresos
{
    public class FichaEgresoUpdateDTO
    {
        public DateTime? Fecha { get; set; }
        public string? Mes { get; set; }
        public int? NombreEgreso { get; set; }  // si cambias el tipo
        public string? Descripcion { get; set; }
        public decimal? Importe { get; set; }
        public string? Foto { get; set; }
    }
}
