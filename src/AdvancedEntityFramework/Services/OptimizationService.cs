using AdvancedEntityFramework.Data;
using AdvancedEntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEntityFramework.Services;

public class OptimizationService
{
    private readonly OptimizationDbContext _context;

    // COMPILED QUERY:
    // Compiles the LINQ expression tree to a delegate once. 
    // Massive perf boost for high-frequency small queries (e.g. FindById).
    private static readonly Func<OptimizationDbContext, int, Task<Order?>> _getOrderById =
        EF.CompileAsyncQuery((OptimizationDbContext db, int id) =>
            db.Orders
                .Include(o => o.Items)
                .AsSplitQuery() // 2. SPLIT QUERY: Avoids Cartesian Explosion
                .AsNoTracking() // 3. NO TRACKING: Faster if read-only
                .FirstOrDefault(o => o.Id == id));

    public OptimizationService(OptimizationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetOrderFastAsync(int id)
    {
        // API invoking the compiled delegate
        return await _getOrderById(_context, id);
    }

    public async Task CreateOrderBatchAsync(int count)
    {
        // 4. BATCHING
        // EF Core automatically batches INSERTs.
        // Npgsql default batch size is usually high enough, but demonstrating logic.
        
        var orders = new List<Order>();
        for (int i = 0; i < count; i++)
        {
            orders.Add(new Order
            {
                CustomerName = $"Cust-{Guid.NewGuid()}",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem> 
                { 
                    new OrderItem { ProductName = "Item A", Price = 10, Quantity = 1 } 
                }
            });
        }

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();
    }
}
