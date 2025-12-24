// Services/JwtService.cs
using CarWash.Api.DTOs;
using CarWash.Api.Models.Entities;
using CarWash.Api.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CarWash.Api.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration config, ILogger<JwtService> logger)
        {
            _config = config;
            _logger = logger;

            // Log for debugging
            var secret = _config["JWT:Secret"];
            var issuer = _config["JWT:Issuer"];
            _logger.LogInformation($"JWT Configuration - Issuer: {issuer}, Secret Length: {(secret?.Length ?? 0)}");
        }

        public string GenerateToken(string identifier, Guid userId)
        {
            try
            {
                var jwtSection = _config.GetSection("JWT");
                var key = jwtSection["Secret"] ?? throw new ArgumentNullException("JWT:Secret", "JWT Secret is required");
                var issuer = jwtSection["Issuer"] ?? "CarWashApi";
                var audience = jwtSection["Audience"] ?? "CarWashClient";
                var expiresInHours = int.TryParse(jwtSection["ExpiresInHours"], out var hours) ? hours : 24;

                // Validate secret key
                if (key.Length < 32)
                    throw new ArgumentException("JWT Secret must be at least 32 characters");

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, identifier),
                    new Claim("userId", userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()) // Standard claim
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token");
                throw;
            }
        }

        public string GenerateToken(User user)
        {
            var identifier = !string.IsNullOrEmpty(user.Email) ? user.Email :
                            user.MobileNumber?.ToString() ?? user.Id.ToString();
            return GenerateToken(identifier, user.Id);
        }

        public bool ValidateToken(string token, out Guid userId)
        {
            userId = Guid.Empty;

            try
            {
                var jwtSection = _config.GetSection("JWT");
                var key = jwtSection["Secret"] ?? throw new ArgumentNullException("JWT:Secret");
                var issuer = jwtSection["Issuer"] ?? "CarWashApi";
                var audience = jwtSection["Audience"] ?? "CarWashClient";

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                // Try multiple claim names
                var userIdClaim = principal.FindFirst("userId")?.Value
                                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? principal.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedId))
                {
                    userId = parsedId;
                    return true;
                }

                _logger.LogWarning($"User ID not found or invalid in token. Claims: {string.Join(", ", principal.Claims.Select(c => $"{c.Type}:{c.Value}"))}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed");
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

        public AuthResponseDto GenerateAuthResponse(Guid userId, string email, IEnumerable<string> roles)
        {
            var accessToken = GenerateAccessToken(userId, email, roles);
            var refreshToken = GenerateRefreshToken();

            // Get expiration times from config
            var jwtSection = _config.GetSection("JWT");
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(
                int.TryParse(jwtSection["AccessTokenExpirationMinutes"], out var accessMinutes) ? accessMinutes : 60);
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                int.TryParse(jwtSection["RefreshTokenExpirationDays"], out var refreshDays) ? refreshDays : 7);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = accessTokenExpiry,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }

        public string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles)
        {
            try
            {
                var jwtSection = _config.GetSection("JWT");
                var key = jwtSection["Secret"] ?? throw new ArgumentNullException("JWT:Secret");
                var issuer = jwtSection["Issuer"] ?? "CarWashApi";
                var audience = jwtSection["Audience"] ?? "CarWashClient";
                var expiresInMinutes = int.TryParse(jwtSection["AccessTokenExpirationMinutes"], out var minutes) ? minutes : 60;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new Claim("userId", userId.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // Add email claim if available
                if (!string.IsNullOrEmpty(email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, email));
                }

                // Add role claims
                foreach (var role in roles ?? Enumerable.Empty<string>())
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token");
                throw;
            }
        }
    }
}