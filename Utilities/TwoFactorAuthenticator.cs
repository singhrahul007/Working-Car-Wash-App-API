using CarWash.Api.Models.Entities;
using OtpNet;
using System;
using System.Security.Cryptography;

namespace CarWash.Api.Utilities
{
    public static class TwoFactorAuthenticator
    {
        public static string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        public static string GenerateQrCodeUrl(string issuer, string userEmail, string secretKey)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(userEmail)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";
        }

        public static bool VerifyCode(string secretKey, string code)
        {
            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(secretKey));
                return totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(2, 2));
            }
            catch
            {
                return false;
            }
        }

        public static string[] GenerateBackupCodes(int count = 8)
        {
            var codes = new string[count];
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                codes[i] = random.Next(100000, 999999).ToString();
            }

            return codes;
        }
    }
}