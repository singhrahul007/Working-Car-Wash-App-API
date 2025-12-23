using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Models.Entities;
using CarWash.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Services
{
    public class SocialAuthService : ISocialAuthService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SocialAuthService(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<SocialAuth?> GetSocialAuthAsync(string provider, string providerId)
        {
            return await _context.SocialAuths
                .Include(sa => sa.User)
                .FirstOrDefaultAsync(sa => sa.Provider == provider && sa.ProviderId == providerId);
        }

        public async Task<SocialAuth> CreateSocialAuthAsync(Guid userId, string provider, string providerId, string email, string? accessToken = null)
        {
            var socialAuth = new SocialAuth
            {
                UserId = userId,
                Provider = provider,
                ProviderId = providerId,
                Email = email,
                AccessToken = accessToken,
                CreatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow
            };

            _context.SocialAuths.Add(socialAuth);
            await _context.SaveChangesAsync();

            return socialAuth;
        }

        public async Task<User?> GetOrCreateUserFromSocialAsync(string provider, string token)
        {
            switch (provider.ToLower())
            {
                case "google":
                    var googleUser = await VerifyGoogleTokenAsync(token);
                    if (googleUser == null) return null;

                    return await FindOrCreateUserFromSocialInfo(provider, googleUser.Id, googleUser.Email, googleUser.Name);

                case "facebook":
                    var facebookUser = await VerifyFacebookTokenAsync(token);
                    if (facebookUser == null) return null;

                    return await FindOrCreateUserFromSocialInfo(provider, facebookUser.Id, facebookUser.Email, facebookUser.Name);

                case "apple":
                    var appleUser = await VerifyAppleTokenAsync(token);
                    if (appleUser == null) return null;

                    return await FindOrCreateUserFromSocialInfo(provider, appleUser.Id, appleUser.Email, appleUser.Name);

                default:
                    return null;
            }
        }

        public async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // Verify token with Google
                var verificationUrl = $"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={token}";
                var response = await httpClient.GetAsync(verificationUrl);

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(content);

                if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.sub))
                    return null;

                // Get user info
                var userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var userInfoResponse = await httpClient.GetAsync(userInfoUrl);
                if (!userInfoResponse.IsSuccessStatusCode)
                    return null;

                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);

                return userInfo;
            }
            catch
            {
                return null;
            }
        }

        public async Task<FacebookUserInfo?> VerifyFacebookTokenAsync(string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // Verify token and get user info
                var appId = _configuration["Facebook:AppId"];
                var appSecret = _configuration["Facebook:AppSecret"];

                // First verify token
                var verifyUrl = $"https://graph.facebook.com/debug_token?input_token={token}&access_token={appId}|{appSecret}";
                var verifyResponse = await httpClient.GetAsync(verifyUrl);

                if (!verifyResponse.IsSuccessStatusCode)
                    return null;

                var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
                var verifyResult = JsonSerializer.Deserialize<FacebookVerifyResponse>(verifyContent);

                if (verifyResult?.data?.is_valid != true)
                    return null;

                // Get user info
                var userInfoUrl = $"https://graph.facebook.com/v12.0/me?fields=id,name,email,first_name,last_name,picture&access_token={token}";
                var userInfoResponse = await httpClient.GetAsync(userInfoUrl);

                if (!userInfoResponse.IsSuccessStatusCode)
                    return null;

                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<FacebookUserInfo>(userInfoContent);

                return userInfo;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AppleUserInfo?> VerifyAppleTokenAsync(string token)
        {
            try
            {
                // For Apple Sign In, we need to validate the JWT token
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Verify the token signature, issuer, audience, etc.
                // This is simplified - in production, you need to:
                // 1. Get Apple's public keys
                // 2. Verify the token signature
                // 3. Check issuer and audience

                var claims = jwtToken.Claims;

                var userId = claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                var email = claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var name = claims.FirstOrDefault(c => c.Type == "name")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return null;

                return new AppleUserInfo
                {
                    Id = userId,
                    Email = email ?? "",
                    Name = name ?? "",
                    EmailVerified = claims.Any(c => c.Type == "email_verified" && c.Value == "true")
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<User?> FindOrCreateUserFromSocialInfo(string provider, string providerId, string email, string name)
        {
            // Check if social auth exists
            var socialAuth = await GetSocialAuthAsync(provider, providerId);
            if (socialAuth != null)
            {
                socialAuth.LastUsed = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return socialAuth.User;
            }

            // Check if user with email exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                // Link social auth to existing user
                await CreateSocialAuthAsync(existingUser.Id, provider, providerId, email);
                return existingUser;
            }

            // Create new user
            var user = new User
            {
                Email = email,
                FullName = name,
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create social auth
            await CreateSocialAuthAsync(user.Id, provider, providerId, email);

            return user;
        }

        // Helper classes for JSON deserialization
        private class GoogleTokenInfo
        {
            public string? sub { get; set; }
            public string? email { get; set; }
            public string? name { get; set; }
        }

        private class FacebookVerifyResponse
        {
            public FacebookVerifyData? data { get; set; }
        }

        private class FacebookVerifyData
        {
            public bool is_valid { get; set; }
            public string? user_id { get; set; }
        }
    }
}