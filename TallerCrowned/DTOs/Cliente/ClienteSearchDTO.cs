namespace TallerCrowned.DTOs.Cliente
{
    public class ClienteSearchDTO
    {
        public string? Search { get; set; }
        public string? Matricula { get; set; }
        public string? Telefono { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
