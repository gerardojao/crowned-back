namespace TallerCrowned.DTOs.Cliente
{
    public class ClienteCreateDTO
    {
        public string Nombre { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string? Email { get; set; }
        public string? Direccion { get; set; }

        public string Matricula { get; set; } = null!;
        public string? Marca { get; set; }
        public string Modelo { get; set; } = null!;

        public int? Kilometraje { get; set; }

        public string? Observaciones { get; set; }
    }
}
