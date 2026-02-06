using AdvancedEntityFramework.Data;
using AdvancedEntityFramework.Entities;
using System.Text.Json;

namespace AdvancedEntityFramework.Services;

public class OrderService
{
    private readonly OptimizationDbContext _context;

    public OrderService(OptimizationDbContext context)
    {
        _context = context;
    }

    public async Task PlaceOrderWithOutboxAsync(string customer, decimal amount)
    {
        // EXECUTION STRATEGY handles standard retries on commit
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Business Logic
                var order = new Order
                {
                    CustomerName = customer,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Get ID

                // 2. Outbox Message (Atomic with Order)
                var message = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredOn = DateTime.UtcNow,
                    Type = "OrderCreated",
                    Content = JsonSerializer.Serialize(new { order.Id, TotalAmount = amount })
                };
                _context.Outbox.Add(message);

                // 3. Commit both
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
