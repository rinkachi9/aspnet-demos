using System.Diagnostics;
using System.Text;
using Demo.Common.Messaging;
using KafkaFlow;

namespace Demo.Worker.Middlewares;

// Middleware to extract trace context from Kafka headers
public class TracingMiddleware : IMessageMiddleware
{
    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var headers = context.Headers;
        var carrier = new Dictionary<string, string>();
        foreach (var h in headers)
        {
            try { carrier[h.Key] = Encoding.UTF8.GetString(h.Value); } catch { }
        }

        var propagator = System.Diagnostics.Propagators.DistributedContextPropagator.Current;
        var parentContext = propagator.Extract(default, carrier, (c, k) => c.TryGetValue(k, out var v) ? new[] { v } : Array.Empty<string>());

        using var activity = Common.Telemetry.Telemetry.ActivitySource.StartActivity(
            "ConsumeMessage",
            ActivityKind.Consumer,
            parentContext.ActivityContext);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination", Topics.Sample);

        await next(context);
    }
}
