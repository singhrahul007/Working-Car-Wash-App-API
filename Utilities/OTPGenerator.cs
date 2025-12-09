using System.Security.Cryptography;

namespace CarWash.Api.Utilities
{
    public static class OTPGenerator
    {
        public static string GenerateOTP(int length = 6)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);

            var random = BitConverter.ToUInt32(bytes, 0);
            var otp = (random % (uint)Math.Pow(10, length)).ToString($"D{length}");

            return otp;
        }

        public static string GenerateNumericOTP(int length = 6)
        {
            var random = new Random();
            var otp = string.Empty;

            for (int i = 0; i < length; i++)
            {
                otp += random.Next(0, 10).ToString();
            }

            return otp;
        }

        public static string GenerateAlphaNumericOTP(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var otp = new char[length];

            for (int i = 0; i < length; i++)
            {
                otp[i] = chars[random.Next(chars.Length)];
            }

            return new string(otp);
        }
        public static bool IsOtpValid(DateTime generatedAt, DateTime verifiedAt, int validityMinutes = 5)
        {
            return (verifiedAt - generatedAt).TotalMinutes <= validityMinutes;
        }
    }

}