using DataPersistencePatterns.Data;
using DataPersistencePatterns.Domain.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. EF Core Configuration
// NodaTime: Handles timestamps correctly (Instant -> text/timestamptz)
// SnakeCaseNamingConvention: Maps OrderId -> order_id
// SplitQuery: Avoids cartesian explosion in 1:N selects
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Postgres") 
                           ?? "Host=localhost;Port=5433;Database=advanced_playground;Username=admin;Password=password";

    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseNodaTime();
        // EnableRetryOnFailure: Transient fault handling (Resilience)
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3); 
    })
    .UseSnakeCaseNamingConvention();

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. Migration & Seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Wait for DB to be ready (naive delay for docker compose up)
    await Task.Delay(2000);
    
    // Auto-migrate
    await db.Database.MigrateAsync();
    // Seed Data
    DbSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. Endpoints

// Optimized: AsNoTracking + AsSplitQuery
app.MapGet("/orders/optimized", async (AppDbContext db) =>
{
    // AsNoTracking: Reducing memory overhead for read-only
    // AsSplitQuery: Separates query into multiple SQL statements to avoid data duplication in join
    return await db.Orders
        .AsNoTracking()
        .Include(o => o.Items)
        .AsSplitQuery()
        .Take(10)
        .ToListAsync();
})
.WithOpenApi();

// Compiled Query: High Throughput
app.MapGet("/orders/compiled/{amount}", (decimal amount, AppDbContext db) =>
{
    // Uses pre-compiled delegate
    return CompiledQueries.OrdersAboveAmount(db, amount);
})
.WithOpenApi();

app.MapGet("/orders/{id}", (Guid id, AppDbContext db) =>
{
    var order = CompiledQueries.GetOrderById(db, id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
})
.WithOpenApi();

app.Run();

// Expose Program for IntegrationTests
public partial class Program { }
