
using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.DTOs
{
    public class LoginDto
    {
        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [MaxLength(6)]
        public string OtpCode { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        [MaxLength(50)]
        public string LoginType { get; set; } = "mobile"; // mobile, email, social
    }

    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }

    public class SendOtpDto
    {
        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string Purpose { get; set; } // login, signup, forgot-password
    }

    public class TwoFactorVerifyDto
    {
        [Required]
        public string Code { get; set; }
    }

    public class TwoFactorSetupDto
    {
        public bool Enable { get; set; }
        public string Method { get; set; } // "authenticator" or "sms"
        public string CurrentPassword { get; set; }
    }

    public class SocialLoginDto
    {
        [Required]
        public string Provider { get; set; } // google, facebook, apple
        [Required]
        public string Token { get; set; }
    }
    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public bool VerifiedEmail { get; set; }
    }

    public class FacebookUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public FacebookPicture? Picture { get; set; }
    }
    public class FacebookPicture
    {
        public FacebookPictureData? Data { get; set; }
    }
    public class FacebookPictureData
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public bool IsSilhouette { get; set; }
        public string? Url { get; set; }
    }

    public class AppleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
    }

}