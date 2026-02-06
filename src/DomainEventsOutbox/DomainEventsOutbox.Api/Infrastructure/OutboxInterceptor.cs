using System.Text.Json;
using DomainEventsOutbox.Api.Domain;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DomainEventsOutbox.Api.Infrastructure;

public class OutboxInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var entities = dbContext.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var outboxMessages = entities
            .SelectMany(x => x.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOn = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()) // Save full type info if needed
            })
            .ToList();

        if (outboxMessages.Any())
        {
            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
            
            // Clear domain events so they don't fire again
            entities.ForEach(e => e.ClearDomainEvents());
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
