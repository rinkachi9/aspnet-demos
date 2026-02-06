using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Basic Config since we don't have separate FluentConfiguration files in this concise example
        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.OwnsOne(o => o.ShippingAddress); // Value Object mapped as Owned Type
            b.HasMany(o => o.Items).WithOne().OnDelete(DeleteBehavior.Cascade);
        });
        
        // Use SnakeCase naming convention automatically via EFCore.NamingConventions package
    }
}
