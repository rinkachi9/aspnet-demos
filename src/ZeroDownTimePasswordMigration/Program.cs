using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using ZeroDownTimePasswordMigration.Auth;
using ZeroDownTimePasswordMigration.Data;
using ZeroDownTimePasswordMigration.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Feature Management
builder.Services.AddFeatureManagement();

// 2. Keyed Services for Password Hashing
builder.Services.AddKeyedSingleton<ICustomPasswordHasher, Pbkdf2PasswordHasher>("legacy");
builder.Services.AddKeyedSingleton<ICustomPasswordHasher, Argon2PasswordHasher>("modern");

// 3. Application Services
builder.Services.AddSingleton<InMemoryUserRepository>();
builder.Services.AddScoped<LoginService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed Data (Hack for demo: seeding with Legacy Hasher)
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<InMemoryUserRepository>();
    var legacyHasher = scope.ServiceProvider.GetRequiredKeyedService<ICustomPasswordHasher>("legacy");
    repo.Seed(legacyHasher);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ENDPOINTS

app.MapPost("/login", async ([FromBody] LoginRequest req, LoginService auth) =>
{
    var success = await auth.LoginAsync(req.Username, req.Password);
    return success ? Results.Ok("Logged In") : Results.Unauthorized();
});

app.MapGet("/users", (InMemoryUserRepository repo) =>
{
    // WARNING: Exposing hashes for DEMO PURPOSES ONLY to visualize migration
    return Results.Ok(repo.GetAll());
});

app.Run();

public record LoginRequest(string Username, string Password);
