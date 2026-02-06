using Refit;

namespace RefitClient.Api.Clients;

public record CreateUserRequest(string FullName, string Email, DateTime DateOfBirth);
public record UserResponse(Guid Id, string FullName, string Email);

public interface IUserClient
{
    // Define the endpoint contract
    // This will target the Gateway which routes to MinimalApiPipeline
    [Post("/api/users")]
    Task<UserResponse> CreateUserAsync([Body] CreateUserRequest request);
    
    [Get("/api/users/{id}")]
    Task<UserResponse> GetUserAsync(Guid id);
}
