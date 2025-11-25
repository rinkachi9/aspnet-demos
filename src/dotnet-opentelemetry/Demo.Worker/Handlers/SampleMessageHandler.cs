using System.Diagnostics;
using Demo.Common.Messaging.Contracts;
using Demo.Common.Telemetry;
using KafkaFlow;
using Serilog;

namespace Demo.Worker.Handlers;

public class SampleMessageHandler : IMessageHandler<SampleMessage>
{
    public async Task Handle(IMessageContext context, SampleMessage message)
    {
        var activity = Activity.Current;
        activity?.SetTag("messaging.message_id", message.Id);
        activity?.SetTag("messaging.message_payload_length", message.Payload.Length);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await Task.Delay(Random.Shared.Next(50, 250), context.ConsumerContext.WorkerStopped);

            Log.Information(
                "Processed message {MessageId} at offset {Offset} / partition {Partition}",
                message.Id,
                context.ConsumerContext.Offset,
                context.ConsumerContext.Partition);
        }
        finally
        {
            stopwatch.Stop();
            Metrics.MessageProcessingDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
