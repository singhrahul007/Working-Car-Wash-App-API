// Update CarWash.Api.Services.IJwtService to match
using CarWash.Api.DTOs;
using CarWash.Api.Entities;

namespace CarWash.Api.Services
{
    public interface IJwtService
    {
        string GenerateToken(string identifier, Guid userId);
        string GenerateToken(User user);
        bool ValidateToken(string token, out Guid userId);
        string GenerateRefreshToken();
        string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);
        AuthResponseDto GenerateAuthResponse(Guid userId, string email, IEnumerable<string> roles);
    }
}