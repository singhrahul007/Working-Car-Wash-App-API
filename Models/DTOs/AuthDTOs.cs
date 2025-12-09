
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
   
  
}