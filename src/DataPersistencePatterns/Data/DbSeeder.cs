using DataPersistencePatterns.Data;
using DataPersistencePatterns.Domain.Entities;
using NodaTime;

namespace DataPersistencePatterns.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Orders.Any()) return;

        var clock = SystemClock.Instance;
        var now = clock.GetCurrentInstant();

        var orders = new List<Order>();

        // Generate 100 orders with items
        for (int i = 0; i < 100; i++)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = $"Customer {i + 1}",
                CreatedAt = now.Minus(Duration.FromDays(i)),
                Items = new List<OrderItem>
                {
                    new() { ProductName = "Item A", Quantity = 1, UnitPrice = 10.00m },
                    new() { ProductName = "Item B", Quantity = 2, UnitPrice = 20.50m }
                }
            };
            order.TotalAmount = order.Items.Sum(x => x.Quantity * x.UnitPrice);
            orders.Add(order);
        }

        context.Orders.AddRange(orders);
        context.SaveChanges();
    }
}
