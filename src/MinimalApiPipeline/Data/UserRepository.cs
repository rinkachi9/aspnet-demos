using System.Collections.Concurrent;
using MinimalApiPipeline.Models;

namespace MinimalApiPipeline.Data;

public interface IUserRepository
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email);
    Task AddAsync(UserDto user);
}

public class InMemoryUserRepository : IUserRepository
{
    private static readonly ConcurrentDictionary<Guid, UserDto> _users = new();

    public Task<UserDto?> GetByIdAsync(Guid id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        var exists = _users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task AddAsync(UserDto user)
    {
        _users.TryAdd(user.Id, user);
        return Task.CompletedTask;
    }
}
