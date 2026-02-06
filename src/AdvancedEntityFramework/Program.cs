using AdvancedEntityFramework.Data;
using AdvancedEntityFramework.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB CONFIGURATION
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<OptimizationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // RETRY STRATEGY (Resiliency)
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3, 
            maxRetryDelay: TimeSpan.FromSeconds(5), 
            errorCodesToAdd: null);
        
        // BATCHING (Optimization)
        npgsqlOptions.MinBatchSize(1);
        npgsqlOptions.MaxBatchSize(100);
    });
    
    // Detailed Errors for Dev
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// IDbContextFactory for Parallel execution
builder.Services.AddDbContextFactory<OptimizationDbContext>(options => 
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<OptimizationService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure DB Created (Demo only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OptimizationDbContext>();
    // db.Database.EnsureCreated(); // Normally use Migrations
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/orders/fast/{id}", async (int id, OptimizationService service) => 
{
    var order = await service.GetOrderFastAsync(id);
    return order != null ? Results.Ok(order) : Results.NotFound();
});

app.MapPost("/orders/batch", async (int count, OptimizationService service) => 
{
    await service.CreateOrderBatchAsync(count);
    return Results.Ok($"{count} orders created in batch.");
});

app.MapPost("/orders/outbox", async (string customer, OrderService service) => 
{
    await service.PlaceOrderWithOutboxAsync(customer, 99.99m);
    return Results.Ok("Order placed with Outbox event.");
});

app.MapGet("/dashboard/{userId}", async (int userId, DashboardService service) => 
{
    var dashboard = await service.GetDashboardAsync(userId);
    return Results.Ok(dashboard);
});

app.Run();
