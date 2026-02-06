using NodaTime;

namespace DataPersistencePatterns.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    
    // NodaTime type for UTC timestamp
    public Instant CreatedAt { get; set; }

    // Navigation
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Foreign Key
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
}
