namespace TallerCrowned.DTOs.Cliente
{
    public class ClienteUpdateDTO
    {
        public string? Nombre { get; set; }
        public string? Dni { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }

        public string? Matricula { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int? Anio { get; set; }
        public int? Kilometraje { get; set; }

        public string? Observaciones { get; set; }
    }
}
