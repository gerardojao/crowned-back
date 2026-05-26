using Microsoft.AspNetCore.Identity;

namespace FamilyApp.Services
{
    public interface IPasswordService
    {
        string Hash(string password);
        bool Verify(string hash, string password);
    }

    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<string> _hasher = new();

        public string Hash(string password) => _hasher.HashPassword("", password);

        public bool Verify(string hash, string password)
            => _hasher.VerifyHashedPassword("", hash, password) is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
