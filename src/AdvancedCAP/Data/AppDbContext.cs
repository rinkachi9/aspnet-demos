using AdvancedCAP.Domain;
using Microsoft.EntityFrameworkCore;

namespace AdvancedCAP.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // CAP tables (cap.published, cap.received) are managed by CAP automatically, 
        // but we need ensure EntityFramework doesn't interfere.
    }
}
