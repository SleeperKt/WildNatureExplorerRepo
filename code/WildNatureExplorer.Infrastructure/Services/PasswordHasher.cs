using WildNatureExplorer.Application.Interfaces.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace WildNatureExplorer.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    // Format: v1.{iterations}.{base64(salt)}.{base64(hash)}
    // Versioned prefix allows future re-hashing without breaking existing users.
    private const string CurrentVersion = "v1";
    private const int CurrentIterations = 600_000;
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

    public string HashPassword(string password)
    {
        byte[] salt = new byte[SaltSizeBytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password,
            salt,
            Prf,
            CurrentIterations,
            HashSizeBytes));

        return $"{CurrentVersion}.{CurrentIterations}.{Convert.ToBase64String(salt)}.{hashed}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword)) return false;

        var parts = hashedPassword.Split('.');

        // Legacy format (v0): "{base64(salt)}.{base64(hash)}" with 10 000 iterations.
        // Kept for backward compatibility with users created before the upgrade.
        if (parts.Length == 2)
        {
            return VerifyLegacy(password, parts[0], parts[1]);
        }

        // Current format: v1.{iterations}.{salt}.{hash}
        if (parts.Length == 4 && parts[0] == CurrentVersion && int.TryParse(parts[1], out var iterations))
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = parts[3];

            string attempted = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password,
                salt,
                Prf,
                iterations,
                HashSizeBytes));

            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(attempted),
                Convert.FromBase64String(expected));
        }

        return false;
    }

    private static bool VerifyLegacy(string password, string saltB64, string hashB64)
    {
        var salt = Convert.FromBase64String(saltB64);
        string attempted = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password,
            salt,
            Prf,
            10_000,
            HashSizeBytes));
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(attempted),
            Convert.FromBase64String(hashB64));
    }
}

