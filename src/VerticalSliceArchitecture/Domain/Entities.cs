namespace VerticalSliceArchitecture.Domain;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Order> Orders { get; set; } = new();
    public string Preferences { get; set; } = "{}";
}

public class Order
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public DateTime Date { get; set; }
}

public class AppDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<AppDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public Microsoft.EntityFrameworkCore.DbSet<User> Users { get; set; }
    public Microsoft.EntityFrameworkCore.DbSet<Order> Orders { get; set; }
}
