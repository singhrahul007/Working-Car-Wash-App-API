
using CarWash.Api.DTOs;
using CarWash.Api.Entities;
using CarWash.Api.Models.Entities;

namespace CarWash.Api.Services.Interfaces
{
    public interface ISocialAuthService
    {
        Task<SocialAuth?> GetSocialAuthAsync(string provider, string providerId);
        Task<SocialAuth> CreateSocialAuthAsync(Guid userId, string provider, string providerId, string email, string? accessToken = null);
        Task<User?> GetOrCreateUserFromSocialAsync(string provider, string token);
        Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string token);
        Task<FacebookUserInfo?> VerifyFacebookTokenAsync(string token);
        Task<AppleUserInfo?> VerifyAppleTokenAsync(string token);
    }

   
}