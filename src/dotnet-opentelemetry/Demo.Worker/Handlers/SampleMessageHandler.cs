using System.Text;
using KafkaFlow;
using Serilog;

namespace Demo.Worker.Handlers;

public class SampleMessageHandler : IMessageMiddleware
{
    private static readonly Random Rng = new();

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var json = Encoding.UTF8.GetString(context.Message.Value as byte[] ?? Array.Empty<byte>());
        Log.Information("Consumed raw message: {Value}", json);

        // Simulate some processing work
        await Task.Delay(Rng.Next(50, 250));

        Log.Information("Processed message offset {Offset}", context.ConsumerContext.Offset);

        await next(context);
    }
}
