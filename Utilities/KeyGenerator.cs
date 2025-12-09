// Utils/KeyGenerator.cs
using System.Security.Cryptography;
using System.Text;

namespace CarWash.Api.Utils
{
    public static class KeyGenerator
    {
        /// <summary>
        /// Generates a secure random key for JWT tokens
        /// </summary>
        /// <param name="length">Length in bytes (default 64 bytes = 512 bits)</param>
        /// <returns>Base64 encoded key</returns>
        public static string GenerateJwtSecretKey(int length = 64)
        {
            using var rng = RandomNumberGenerator.Create();
            var keyBytes = new byte[length];
            rng.GetBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }

        /// <summary>
        /// Generates an API key with prefix
        /// </summary>
        /// <param name="prefix">Key prefix (e.g., "cwsk_" for CarWash Secret Key)</param>
        /// <param name="length">Length of the random part in bytes</param>
        /// <returns>Formatted API key</returns>
        public static string GenerateApiKey(string prefix = "cwsk_", int length = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            var keyBytes = new byte[length];
            rng.GetBytes(keyBytes);
            var base64Key = Convert.ToBase64String(keyBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            return $"{prefix}{base64Key}";
        }

        /// <summary>
        /// Generates a secure password reset token
        /// </summary>
        public static string GeneratePasswordResetToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        /// <summary>
        /// Generates a verification code (numeric)
        /// </summary>
        public static string GenerateVerificationCode(int length = 6)
        {
            using var rng = RandomNumberGenerator.Create();
            var code = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                code.Append(RandomNumberGenerator.GetInt32(0, 10));
            }
            return code.ToString();
        }

        /// <summary>
        /// Generates a secure refresh token
        /// </summary>
        public static string GenerateRefreshToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[64];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }

        /// <summary>
        /// Validates if a string is a valid base64 key
        /// </summary>
        public static bool IsValidBase64Key(string key, int minLength = 16)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            try
            {
                var bytes = Convert.FromBase64String(key);
                return bytes.Length >= minLength;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a key pair for asymmetric encryption (RSA)
        /// </summary>
        public static (string PublicKey, string PrivateKey) GenerateRsaKeyPair(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);

            var publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            var privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());

            return (publicKey, privateKey);
        }
    }
}