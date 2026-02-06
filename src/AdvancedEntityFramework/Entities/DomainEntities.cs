using System.ComponentModel.DataAnnotations;

namespace AdvancedEntityFramework.Entities;

public class Order
{
    public int Id { get; set; }
    public required string CustomerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    // Optimistic Concurrency Token (Postgres xmin system column)
    [Timestamp]
    public uint Version { get; set; }
    
    // Foreign Key for Dashboard Demo
    public int UserId { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public List<Order> Orders { get; set; } = new();
}

public class SystemLog
{
    public int Id { get; set; }
    public required string Message { get; set; }
    public required string Level { get; set; }
    public DateTime Timestamp { get; set; }
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public required string Type { get; set; }
    public required string Content { get; set; }
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
}
