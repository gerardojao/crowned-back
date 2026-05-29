namespace TallerCrowned.Models
{
    public class OrdenTrabajo
    {
        public int Id { get; set; }

        public string Cliente { get; set; } = null!;
        public string? Telefono { get; set; }

        public string Matricula { get; set; } = null!;
        public string? Marca { get; set; }
        public string Modelo { get; set; } = null!;
        public int? Kilometraje { get; set; }

        public DateTime Fecha { get; set; }

        public string Trabajo { get; set; } = null!;

        public decimal Repuestos { get; set; }
        public decimal ManoObra { get; set; }

        public string Estado { get; set; } = "Recibido";

        public string? Observaciones { get; set; }
        public bool Facturada { get; set; }

        public bool Eliminado { get; set; }
    }
}
