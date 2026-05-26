namespace FamilyApp.DTOs.Auth
{
    public class ResetPasswordDto
    {
        public string Email { get; set; } = default!;
        public string Token { get; set; } = default!;      // token recibido por email
        public string NewPassword { get; set; } = default!;
    }
}
