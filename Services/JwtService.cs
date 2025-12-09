// Services/JwtService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CarWash.Api.Entities;
using CarWash.Api.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace CarWash.Api.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string identifier, Guid userId)
        {
            var key = _config["JWT:Secret"] ?? "change_this_in_production";
            var issuer = _config["JWT:Issuer"] ?? "CarWashApi";
            var audience = _config["JWT:Audience"] ?? "CarWashClient";
            var expiresInHours = int.TryParse(_config["JWT:ExpiresInHours"], out var hours) ? hours : 24;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, identifier),
                new Claim("userId", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiresInHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateToken(User user)
        {
            var identifier = !string.IsNullOrEmpty(user.Email) ? user.Email : user.MobileNumber ?? "unknown";
            return GenerateToken(identifier, user.Id);
        }

        public bool ValidateToken(string token, out Guid userId)
        {
            userId = Guid.Empty;

            try
            {
                var key = _config["JWT:Secret"] ?? "change_this_in_production";
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["JWT:Issuer"] ?? "CarWashApi",
                    ValidateAudience = true,
                    ValidAudience = _config["JWT:Audience"] ?? "CarWashClient",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                var userIdClaim = principal.FindFirst("userId")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedId))
                {
                    userId = parsedId;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}