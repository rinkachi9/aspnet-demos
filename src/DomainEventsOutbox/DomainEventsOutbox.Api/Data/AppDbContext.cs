using DomainEventsOutbox.Api.Domain;
using DomainEventsOutbox.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DomainEventsOutbox.Api.Data;

public class AppDbContext : DbContext
{
    private readonly OutboxInterceptor _outboxInterceptor;

    public AppDbContext(DbContextOptions<AppDbContext> options, OutboxInterceptor outboxInterceptor) : base(options)
    {
        _outboxInterceptor = outboxInterceptor;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_outboxInterceptor);
        base.OnConfiguring(optionsBuilder);
    }
}
