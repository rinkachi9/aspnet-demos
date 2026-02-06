using Microsoft.Extensions.Http.Resilience;
using Refit;
using RefitClient.Api.Clients;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Refit Client with Resilience
builder.Services
    .AddRefitClient<IUserClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5278")) // Gateway URL
    .AddStandardResilienceHandler(); // Adds Retry, CircuitBreaker, Timeout, RateLimiter automatically!

var app = builder.Build();

// 2. Proxy Endpoint to test the Internal Client
app.MapPost("/proxy/users", async (IUserClient client, CreateUserRequest request) =>
{
    try 
    {
        var response = await client.CreateUserAsync(request);
        return Results.Ok(response);
    }
    catch (ApiException ex)
    {
        return Results.Problem(ex.Content, statusCode: (int)ex.StatusCode);
    }
});

app.Run();
