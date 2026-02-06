using MinimalApiPipeline.Filters;
using MinimalApiPipeline.Models;
using MinimalApiPipeline.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MinimalApiPipeline.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateUser)
             .WithName("CreateUser")
             .WithOpenApi()
             .AddEndpointFilter<ValidationFilter<CreateUserRequest>>(); 
        
        group.MapGet("/{id:guid}", GetUserById)
             .WithName("GetUserById")
             .WithOpenApi();

        return group;
    }

    // TypedResults usage with Service
    // Note: We don't need to return Conflict/BadRequest explicitly here because Middleware handles DomainException
    public static async Task<Results<Created<UserDto>, ValidationProblem, BadRequest>> CreateUser(
        [FromBody] CreateUserRequest request,
        [FromServices] IUserService userService)
    {
        var user = await userService.CreateUserAsync(request);
        return TypedResults.Created($"/api/users/{user.Id}", user);
    }

    public static async Task<Results<Ok<UserDto>, NotFound>> GetUserById(
        Guid id,
        [FromServices] IUserService userService)
    {
        var user = await userService.GetUserAsync(id);

        if (user is null) 
            return TypedResults.NotFound();

        return TypedResults.Ok(user);
    }
}
