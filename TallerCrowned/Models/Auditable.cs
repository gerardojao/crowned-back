// Common/Auditable.cs
public abstract class Auditable
{
    public bool Activo { get; set; } = true;

    public string? UsuarioCreacion { get; set; }
    public DateTime FechaCreacion { get; set; }

    public string? UsuarioModificacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}


