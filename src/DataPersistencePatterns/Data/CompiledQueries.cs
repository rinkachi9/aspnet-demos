using DataPersistencePatterns.Data;
using DataPersistencePatterns.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataPersistencePatterns.Data;

public static class CompiledQueries
{
    // Pre-compiles the query delegate.
    // Benefit: query plan is cached and reused, bypassing query pipeline overhead.
    public static readonly Func<AppDbContext, decimal, IEnumerable<Order>> OrdersAboveAmount =
        EF.CompileQuery((AppDbContext context, decimal amount) =>
            context.Orders.AsNoTracking().Where(o => o.TotalAmount > amount));

    public static readonly Func<AppDbContext, Guid, Order?> GetOrderById =
        EF.CompileQuery((AppDbContext context, Guid id) =>
            context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id));
}
