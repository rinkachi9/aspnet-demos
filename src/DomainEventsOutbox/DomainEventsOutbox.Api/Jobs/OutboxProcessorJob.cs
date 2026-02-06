using System.Text.Json;
using DomainEventsOutbox.Api.Data;
using DomainEventsOutbox.Api.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DomainEventsOutbox.Api.Jobs;

// Run frequently (every 5-10s)
[DisallowConcurrentExecution]
public class OutboxProcessorJob : IJob
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OutboxProcessorJob> _logger;

    public OutboxProcessorJob(AppDbContext dbContext, IPublishEndpoint publishEndpoint, ILogger<OutboxProcessorJob> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync();

        if (!messages.Any()) return;

        _logger.LogInformation("Processing {Count} outbox messages...", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                // In a real generic outbox, you'd use reflection to deserialize to specific Type or use a generic notification.
                // For this demo, we know we only have UserRegisteredEvent.
                
                if (message.Type == nameof(UserRegisteredEvent))
                {
                    var domainEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(message.Content);
                    if (domainEvent != null)
                    {
                        await _publishEndpoint.Publish(domainEvent); // Publish to RabbitMQ
                        _logger.LogInformation("Published: {Type}", message.Type);
                    }
                }
                else
                {
                     _logger.LogWarning("Unknown message type: {Type}", message.Type);
                }

                message.ProcessedOn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed to process message {Id}", message.Id);
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}
