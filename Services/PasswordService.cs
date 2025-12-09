// Services/PasswordService.cs
using CarWash.Api.Interfaces;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace CarWash.Api.Services
{
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

        public string HashPassword(string password, out byte[] salt)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            // Generate a random salt
            salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Hash the password with the salt
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithm,
                outputLength: KeySize
            );

            // Combine salt and hash for storage
            var hashBytes = new byte[SaltSize + KeySize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            try
            {
                // Extract the stored hash bytes
                var hashBytes = Convert.FromBase64String(storedHash);

                // Get the hash part (skip the salt part)
                var hash = new byte[KeySize];
                Array.Copy(hashBytes, SaltSize, hash, 0, KeySize);

                // Compute hash of the provided password with the stored salt
                var testHash = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: storedSalt,
                    iterations: Iterations,
                    hashAlgorithm: HashAlgorithm,
                    outputLength: KeySize
                );

                // Compare the computed hash with the stored hash
                return CryptographicOperations.FixedTimeEquals(hash, testHash);
            }
            catch
            {
                return false;
            }
        }

        public bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            // Check for at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Check for at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Check for at least one digit
            if (!Regex.IsMatch(password, @"\d"))
                return false;

            // Check for at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                return false;

            // Check for no whitespace
            if (Regex.IsMatch(password, @"\s"))
                return false;

            // Check for common weak passwords
            var weakPasswords = new List<string>
            {
                "password", "12345678", "qwerty123", "admin123", "welcome123",
                "password123", "letmein123", "monkey123", "dragon123", "sunshine123"
            };

            if (weakPasswords.Contains(password.ToLower()))
                return false;

            return true;
        }

        // Optional: Additional helper methods
        public (string Hash, byte[] Salt) CreatePasswordHash(string password)
        {
            var hash = HashPassword(password, out var salt);
            return (hash, salt);
        }

        public bool VerifyPassword(string password, string storedHashWithSalt)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHashWithSalt))
                return false;

            try
            {
                var hashBytes = Convert.FromBase64String(storedHashWithSalt);
                if (hashBytes.Length != SaltSize + KeySize)
                    return false;

                // Extract salt from the combined hash
                var salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Extract hash from the combined hash
                var storedHash = new byte[KeySize];
                Array.Copy(hashBytes, SaltSize, storedHash, 0, KeySize);

                // Compute hash of the provided password
                var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: Iterations,
                    hashAlgorithm: HashAlgorithm,
                    outputLength: KeySize
                );

                return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
            }
            catch
            {
                return false;
            }
        }

        public string GenerateRandomPassword(int length = 12)
        {
            if (length < 8) length = 12;

            const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowercase = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@#$%^&*()_-+=<>?";

            var allChars = uppercase + lowercase + digits + special;
            var password = new char[length];

            // Ensure at least one of each required character type
            password[0] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
            password[1] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
            password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

            // Fill the rest with random characters
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
            }

            // Shuffle the password
            for (int i = 0; i < length; i++)
            {
                var randomIndex = RandomNumberGenerator.GetInt32(length);
                (password[i], password[randomIndex]) = (password[randomIndex], password[i]);
            }

            return new string(password);
        }
    }
}