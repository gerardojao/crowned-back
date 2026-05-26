using FamilyApp.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FamilyApp.Services
{
    public interface ITokenService
    {
        string CreateToken(AppUser user, Guid jti, out DateTime expiresAt);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _cfg;
        public TokenService(IConfiguration cfg) => _cfg = cfg;

        public string CreateToken(AppUser user, Guid jti, out DateTime expiresAt)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var issuer = _cfg["Jwt:Issuer"];
            var audience = _cfg["Jwt:Audience"];
            var mins = int.TryParse(_cfg["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            expiresAt = DateTime.UtcNow.AddMinutes(mins);

            var claims = new List<Claim>
            {
                // ✅ Id estándar en JWT
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

                // (Opcional pero útil) redundancia para middlewares que usan NameIdentifier
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                // ✅ Email estándar + versión ClaimTypes
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),

                new Claim(ClaimTypes.Role, user.Role ?? "user"),
                new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
            };

            expiresAt = DateTime.UtcNow.AddHours(12);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                //signingCredentials: creds
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
