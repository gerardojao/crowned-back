namespace TallerCrowned.DTOs.OrdenTrabajo
{
    public class OrdenTrabajoUpdateDto
    {
        public string? Cliente { get; set; }
        public string? Telefono { get; set; }

        public string? Matricula { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int? Kilometraje { get; set; }

        public DateTime? Fecha { get; set; }

        public string? Trabajo { get; set; }

        public decimal? Repuestos { get; set; }
        public decimal? ManoObra { get; set; }

        public string? Estado { get; set; }

        public string? Observaciones { get; set; }
    }
}
