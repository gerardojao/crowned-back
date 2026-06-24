namespace TallerCrowned.DTOs.PreOrdenTrabajo
{
    public class PreOrdenTrabajoUpdateDto
    {
        public string? Cliente { get; set; }
        public string? Dni { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public string? Matricula { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int? Kilometraje { get; set; }

        public DateTime? Fecha { get; set; }
        public DateTime? FechaPrevistaEntrega { get; set; }
        public decimal? TiempoEstimadoHoras { get; set; }

        public string? MotivoRecepcion { get; set; }
        public string? DiagnosticoMecanico { get; set; }
        public string? RepuestosNecesarios { get; set; }
        public string? Observaciones { get; set; }
        public string? Estado { get; set; }
    }
}
