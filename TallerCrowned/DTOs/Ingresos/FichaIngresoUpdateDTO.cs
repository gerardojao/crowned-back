namespace FamilyApp.DTOs.Ingresos
{
    public class FichaIngresoUpdateDTO
    {
        public DateTime? Fecha { get; set; }
        public string? Mes { get; set; }
        public int? NombreIngreso { get; set; }   // si cambia el tipo
        public string? Descripcion { get; set; }
        public decimal? Importe { get; set; }
        public string? Foto { get; set; }
    }
}
