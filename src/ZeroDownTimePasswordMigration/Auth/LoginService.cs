using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using ZeroDownTimePasswordMigration.Data;
using ZeroDownTimePasswordMigration.Services;

namespace ZeroDownTimePasswordMigration.Auth;

public class LoginService(
    InMemoryUserRepository userRepository,
    [FromKeyedServices("modern")] ICustomPasswordHasher modernHasher,
    [FromKeyedServices("legacy")] ICustomPasswordHasher legacyHasher,
    IFeatureManager featureManager,
    ILogger<LoginService> logger)
{
    public async Task<bool> LoginAsync(string username, string password)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            logger.LogWarning("User not found: {Username}", username);
            return false;
        }

        // 1. Try Modern (Argon2) first - Happy Path for new/migrated users
        if (modernHasher.VerifyPassword(user.PasswordHash, password))
        {
            logger.LogInformation("Success: Logged in with Modern Hash for {Username}", username);
            return true;
        }

        // 2. Fallback: Try Legacy (PBKDF2)
        if (legacyHasher.VerifyPassword(user.PasswordHash, password))
        {
            logger.LogWarning("Success: Logged in with LEGACY Hash for {Username}. Checking migration flag...", username);

            // 3. Migration Logic (Feature Flagged)
            if (await featureManager.IsEnabledAsync("MigratePasswords"))
            {
                logger.LogInformation("MIGRATION: Upgrading password for {Username} to Argon2...", username);
                
                var newHash = modernHasher.HashPassword(password);
                user.PasswordHash = newHash;
                
                await userRepository.SaveChangesAsync();
                logger.LogInformation("MIGRATION: Done.");
            }
            
            return true;
        }

        logger.LogError("Failure: Invalid password for {Username}", username);
        return false;
    }
}
