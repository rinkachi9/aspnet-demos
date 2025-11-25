using System.Diagnostics;
using System.Text;
using Demo.Common.Telemetry;
using KafkaFlow;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Demo.Worker.Middlewares;

public class TracingMiddleware : IMessageMiddleware
{
    private readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var carrier = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in context.Headers)
        {
            if (header.Value is null) continue;
            carrier[header.Key] = Encoding.UTF8.GetString(header.Value);
        }

        var parentContext = _propagator.Extract(default, carrier, (c, key) =>
            c.TryGetValue(key, out var value) ? new[] { value } : Array.Empty<string>());

        Baggage.Current = parentContext.Baggage;

        using var activity = Telemetry.ActivitySource.StartActivity(
            "ConsumeMessage",
            ActivityKind.Consumer,
            parentContext.ActivityContext);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination", context.ConsumerContext.Topic);
        activity?.SetTag("messaging.operation", "process");
        activity?.SetTag("messaging.kafka.topic", context.ConsumerContext.Topic);
        activity?.SetTag("messaging.kafka.partition", context.ConsumerContext.Partition);
        activity?.SetTag("messaging.kafka.offset", context.ConsumerContext.Offset);

        await next(context);
    }
}
