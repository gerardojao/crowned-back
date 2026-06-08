namespace TallerCrowned.DTOs.OrdenTrabajo
{
    public class OrdenTrabajoCreateDto
    {
        public string Cliente { get; set; } = null!;
        public string? Dni { get; set; }
        public string? Telefono { get; set; }

        public string Matricula { get; set; } = null!;
        public string? Marca { get; set; }
        public string Modelo { get; set; } = null!;
        public int? Kilometraje { get; set; }

        public DateTime Fecha { get; set; }

        public string Trabajo { get; set; } = null!;

        public decimal Repuestos { get; set; }
        public decimal Cantidad { get; set; } = 1;
        public decimal ManoObra { get; set; }
        public string? ItemsJson { get; set; }

        public string Estado { get; set; } = "Recibido";

        public string? Observaciones { get; set; }
    }
}
