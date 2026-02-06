using AdvancedEntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEntityFramework.Data;

public class OptimizationDbContext : DbContext
{
    public OptimizationDbContext(DbContextOptions<OptimizationDbContext> options) : base(options)
    {
        // Global NoTracking behavior (dangerous if you need change tracking elsewhere, 
        // usually better to keep default and use AsNoTracking per query, but demonstrative here)
        // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OutboxMessage> Outbox { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Map Version to xmin (Postgres system column for concurrency)
        modelBuilder.Entity<Order>()
            .UseXminAsConcurrencyToken();

        // 2. Configure Split Queries globally (Optional, can be per query)
        // modelBuilder.Entity<Order>().HasMany(o => o.Items).WithOne(); // Default
    }
}
