using AdvancedMassTransit.Sagas;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AdvancedMassTransit.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // SAGA STATE
    public DbSet<OrderState> OrderStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SAGA MAP
        modelBuilder.Entity<OrderState>().HasKey(x => x.CorrelationId);

        // OUTBOX CONFIGURATION
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
