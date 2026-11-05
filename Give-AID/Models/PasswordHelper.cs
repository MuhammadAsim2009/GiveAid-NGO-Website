using System.Security.Cryptography;

namespace Give_AID.Models
{
    // Utility helper class for hashing and verifying passwords using PBKDF2
    public static class PasswordHelper
    {
        // Number of PBKDF2 iterations for key derivation
        private const int Iterations = 100000;

        // Size of the cryptographic salt in bytes
        private const int SaltSize = 16;

        // Size of the generated hash in bytes
        private const int HashSize = 32;

        // Generates a salted PBKDF2 hash of the plain text password
        public static string HashPassword(string password)
        {
            // Generate a secure random salt
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Compute the PBKDF2 hash using SHA256
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            // Return salt and hash combined into a single string separated by a colon
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        // Verifies whether an input password matches the stored salted hash
        public static bool VerifyPassword(string password, string storedHash)
        {
            // If either string is missing or invalid format, return false
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var parts = storedHash.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            try
            {
                // Extract salt and original hash bytes
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] originalHash = Convert.FromBase64String(parts[1]);

                // Hash the incoming test password with the exact same salt
                byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    HashSize);

                // Compare the two hashes using fixed-time comparison to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(originalHash, testHash);
            }
            catch
            {
                // In case of parsing error, safely fail authentication
                return false;
            }
        }
    }
}

