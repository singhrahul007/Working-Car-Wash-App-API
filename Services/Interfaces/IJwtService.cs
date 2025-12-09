// Update CarWash.Api.Services.IJwtService to match
using CarWash.Api.Entities;

namespace CarWash.Api.Services
{
    public interface IJwtService
    {
        string GenerateToken(string identifier, Guid userId);
        string GenerateToken(User user);
        bool ValidateToken(string token, out Guid userId);
        string GenerateRefreshToken();
    }
}