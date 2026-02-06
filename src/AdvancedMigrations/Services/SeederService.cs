using DataPersistencePatterns.Data;
using DataPersistencePatterns.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AdvancedMigrations.Services;

public class SeederService(
    AppDbContext dbContext,
    ILogger<SeederService> logger)
{
    public async Task SeedAsync(string environment, string? tag, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting seeding for Environment: {Env}, Tag: {Tag}", environment, tag ?? "None");

        // 1. Basic Seeding (Idempotent)
        if (!dbContext.Orders.Any())
        {
            logger.LogInformation("Seeding initial orders...");
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), CreatedAt = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow.AddDays(-1)), TotalAmount = 100.50m, CustomerName = "Customer A" },
                new Order { Id = Guid.NewGuid(), CreatedAt = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow), TotalAmount = 200.00m, CustomerName = "Customer B" }
            };
            dbContext.Orders.AddRange(orders);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // 2. Conditional Seeding based on Tag
        if (tag == "performance")
        {
            logger.LogInformation("Executing Performance Seeding (1k records)...");
            var perfId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var perfOrder = new Order { Id = perfId, CreatedAt = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow), TotalAmount = 999.99m, CustomerName = "Perf Customer" };
            
            // Check existence
            if (await dbContext.Orders.FindAsync(new object[] { perfId }, cancellationToken) == null) 
            {
                dbContext.Orders.Add(perfOrder);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        logger.LogInformation("Seeding completed.");
    }
}
