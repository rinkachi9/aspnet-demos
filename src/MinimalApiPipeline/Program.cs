using FluentValidation;
using MinimalApiPipeline.Endpoints;
using MinimalApiPipeline.Middleware;
using MinimalApiPipeline.Validations;

using AspireApp.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// 1. Add Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddSingleton<MinimalApiPipeline.Data.IUserRepository, MinimalApiPipeline.Data.InMemoryUserRepository>();
builder.Services.AddScoped<MinimalApiPipeline.Services.IUserService, MinimalApiPipeline.Services.UserService>();

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// 2. Configure Pipeline

// Global Exception Handler (Custom Middleware)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Standard Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// Content Negotiation (Minimal APIs default to JSON, but we can return other formats if needed via custom results)

// 3. Define Endpoints
var apiGroup = app.MapGroup("/api/users")
    .WithTags("Users")
    .WithOpenApi();

apiGroup.MapUserEndpoints();

// Branching Pipeline Example
// For requests starting with /admin, we might want different middleware (e.g., simplistic auth check or specialized logging)
app.MapWhen(context => context.Request.Path.StartsWithSegments("/admin"), branch =>
{
    branch.Use(async (context, next) =>
    {
        // Simple mock check
        if (!context.Request.Headers.ContainsKey("X-Admin-Key"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden: Missing Admin Key");
            return;
        }
        await next();
    });

    branch.Run(async context =>
    {
        await context.Response.WriteAsync("Welcome to Admin Section (Branched Pipeline)");
    });
});

app.Run();
