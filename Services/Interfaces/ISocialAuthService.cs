
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

    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }

    public class FacebookUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
    }

    public class AppleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
    }
}