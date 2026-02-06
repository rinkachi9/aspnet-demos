using Microsoft.EntityFrameworkCore;

namespace AdvancedTuning.Data;

// Must be lightweight and stateless/resetable for Pooling
public class TuningDbContext : DbContext
{
    public TuningDbContext(DbContextOptions<TuningDbContext> options) : base(options)
    {
    }

    // Example entity
    public DbSet<PooledEntity> PooledEntities { get; set; }
}

public class PooledEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}
