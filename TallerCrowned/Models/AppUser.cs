using System.ComponentModel.DataAnnotations;

namespace FamilyApp.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!; // hash PBKDF2

        [MaxLength(60)]
        public string Role { get; set; } = "user"; // "admin" | "user"

        [MaxLength(160)]
        public string? FullName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Control de sesión única por cuenta:
        public Guid? ActiveSessionJti { get; set; }              // jti del JWT activo
        public DateTime? ActiveSessionExpiresAt { get; set; }    // caducidad del token activo
    }
}
