namespace MinimalApiPipeline.Models;

public record UserDto(Guid Id, string FullName, string Email, int Age);
public record CreateUserRequest(string FullName, string Email, int Age);
