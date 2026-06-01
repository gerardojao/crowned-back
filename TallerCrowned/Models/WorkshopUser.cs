namespace TallerCrowned.Models
{
    public class WorkshopUser
    {
        public int Id { get; set; }
        public int WorkshopId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = "owner";
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public Workshop Workshop { get; set; } = null!;
    }
}
