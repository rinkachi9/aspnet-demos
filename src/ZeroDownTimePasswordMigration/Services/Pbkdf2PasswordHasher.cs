using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace ZeroDownTimePasswordMigration.Services;

public interface ICustomPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}

// Key: "legacy"
public class Pbkdf2PasswordHasher : ICustomPasswordHasher
{
    // Simple PBKDF2 implementation for demo
    // Format: pbkdf2|salt|hash
    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        return $"pbkdf2|{Convert.ToBase64String(salt)}|{hashed}";
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (!hashedPassword.StartsWith("pbkdf2|")) return false;

        var parts = hashedPassword.Split('|');
        if (parts.Length != 3) return false;

        var salt = Convert.FromBase64String(parts[1]);
        var originalHash = parts[2];

        string newHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: providedPassword,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000, // Matching iterations
            numBytesRequested: 256 / 8));

        return newHash == originalHash;
    }
}
