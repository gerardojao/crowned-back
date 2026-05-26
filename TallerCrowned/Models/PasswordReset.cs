using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FamilyApp.Models
{
    [Index(nameof(TokenHash), IsUnique = true)]
    public class PasswordReset
    {
        [Key] public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public AppUser User { get; set; } = default!;

        // Hash (SHA256) del token (nunca guardes el token en claro)
        [Required, MaxLength(88)] // base64 SHA256 = 44, por seguridad dejamos margen
        public string TokenHash { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }            // Ej: UtcNow + 30 min
        public DateTime? UsedAt { get; set; }              // null = no usado

        // Metadata opcional
        [MaxLength(64)] public string? RequestIp { get; set; }
        [MaxLength(256)] public string? RequestUserAgent { get; set; }
    }
}
