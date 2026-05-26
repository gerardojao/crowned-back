namespace FamilyApp.DTOs.Ingresos
{
    public class MovimientoDTO
    {
       
            public int Id { get; set; }
            public DateTime? Fecha { get; set; }
            public string? Mes { get; set; }
            public int TipoId { get; set; }
            public string Tipo { get; set; } = string.Empty; // nombre del tipo (Ingreso/Egreso)
            public string? Descripcion { get; set; }
            public decimal Importe { get; set; }
            public string Kind { get; set; } = string.Empty; // "ingreso" | "egreso"
        
    }
}
