using System.Security.Cryptography;
using System.Text;

namespace FamilyApp.Services
{
    public static class SecureToken
    {
        // Crea un token URL-safe (base64url)
        public static string CreateUrlToken(int bytesLen = 32)
        {
            var bytes = RandomNumberGenerator.GetBytes(bytesLen);
            return Base64UrlEncode(bytes);
        }

        // Hash SHA256 en base64
        public static string Sha256Base64(string text)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToBase64String(hash);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
