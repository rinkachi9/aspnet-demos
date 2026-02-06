using AdvancedMassTransit.Contracts;
using MassTransit;

namespace AdvancedMassTransit.Consumers;

public class TelemetryConsumer : IConsumer<OrderTelemetry>
{
    private readonly ILogger<TelemetryConsumer> _logger;

    public TelemetryConsumer(ILogger<TelemetryConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderTelemetry> context)
    {
        // High throughput processing (e.g. save to ClickHouse/Elastic)
        _logger.LogInformation("KAFKA TELEMETRY: Order {Id} Action {Action} Latency {Lat}ms", 
            context.Message.OrderId, 
            context.Message.Action, 
            context.Message.LatencyMs);

        return Task.CompletedTask;
    }
}
