namespace TallerCrowned.DTOs.PreOrdenTrabajo
{
    public class PreOrdenTrabajoCreateDto
    {
        public string Cliente { get; set; } = null!;
        public string? Dni { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public string Matricula { get; set; } = null!;
        public string? Marca { get; set; }
        public string Modelo { get; set; } = null!;
        public int? Kilometraje { get; set; }

        public DateTime Fecha { get; set; }
        public DateTime? FechaPrevistaEntrega { get; set; }
        public decimal? TiempoEstimadoHoras { get; set; }

        public string MotivoRecepcion { get; set; } = null!;
        public string? DiagnosticoMecanico { get; set; }
        public string? RepuestosNecesarios { get; set; }
        public string? Observaciones { get; set; }
    }
}
