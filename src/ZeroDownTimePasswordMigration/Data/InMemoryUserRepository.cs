using ZeroDownTimePasswordMigration.Services;

namespace ZeroDownTimePasswordMigration.Data;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

public class InMemoryUserRepository
{
    private readonly List<User> _users = new();

    public InMemoryUserRepository()
    {
    }

    // Creating this to bypass constructor DI issue for seeding demo
    public void Seed(ICustomPasswordHasher hasher)
    {
         if (!_users.Any())
         {
             _users.Add(new User 
             { 
                 Id = 1, 
                 Username = "legacy_user", 
                 PasswordHash = hasher.HashPassword("password123") // Legacy Hash
             });
             
             _users.Add(new User 
             { 
                 Id = 2, 
                 Username = "legacy_admin", 
                 PasswordHash = hasher.HashPassword("admin123") // Legacy Hash
             });
         }
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        return Task.FromResult(user);
    }

    public Task SaveChangesAsync()
    {
        // In-memory, so nothing to do really. 
        // In real DB: await dbContext.SaveChangesAsync();
        return Task.CompletedTask;
    }
    
    public List<User> GetAll() => _users; 
}
