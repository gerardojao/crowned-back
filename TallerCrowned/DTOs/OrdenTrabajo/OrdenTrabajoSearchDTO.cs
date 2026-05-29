namespace TallerCrowned.DTOs.OrdenTrabajo
{
    public class OrdenTrabajoSearchDto
    {
        public string? Matricula { get; set; }
        public string? Cliente { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
