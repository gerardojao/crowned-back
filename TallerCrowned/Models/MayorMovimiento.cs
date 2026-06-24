using System;

namespace TallerCrowned.Models
{
    public class MayorMovimiento
    {
        public int Id { get; set; }
        public string Cuenta { get; set; } = "";
        public string TipoMovimiento { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Referencia { get; set; } = "";
        public string? Descripcion { get; set; }
        public decimal Importe { get; set; }
        public bool Eliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }
    }
}
