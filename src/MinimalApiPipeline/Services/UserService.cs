using MinimalApiPipeline.Data;
using MinimalApiPipeline.Domain;
using MinimalApiPipeline.Models;

namespace MinimalApiPipeline.Services;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto?> GetUserAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        _logger.LogInformation("Creating user: {Email}", request.Email);

        if (await _repository.ExistsByEmailAsync(request.Email))
        {
            _logger.LogWarning("User with email {Email} already exists.", request.Email);
            throw new UserAlreadyExistsException(request.Email);
        }

        var user = new UserDto(Guid.NewGuid(), request.FullName, request.Email, request.Age);
        await _repository.AddAsync(user);
        
        _logger.LogInformation("User created: {Id}", user.Id);
        return user;
    }

    public async Task<UserDto?> GetUserAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
