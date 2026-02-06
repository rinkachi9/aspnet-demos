using Isopoh.Cryptography.Argon2;

namespace ZeroDownTimePasswordMigration.Services;

// Key: "modern"
public class Argon2PasswordHasher : ICustomPasswordHasher
{
    // Argon2id implementation
    public string HashPassword(string password)
    {
        return Argon2.Hash(password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (hashedPassword.StartsWith("pbkdf2|")) return false; // Not my format

        try 
        {
            return Argon2.Verify(hashedPassword, providedPassword);
        }
        catch 
        {
            return false;
        }
    }
}
