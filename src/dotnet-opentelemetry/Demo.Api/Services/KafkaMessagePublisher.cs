using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Demo.Common.Kafka;
using Demo.Common.Messaging.Contracts;
using Demo.Common.Telemetry;
using KafkaFlow;
using KafkaFlow.Producers;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Serilog;

namespace Demo.Api.Services;

public class KafkaMessagePublisher : IMessagePublisher
{
    private readonly IMessageProducer _producer;
    private readonly TextMapPropagator _propagator;
    private readonly KafkaTopics _topics;

    public KafkaMessagePublisher(IProducerAccessor accessor, KafkaTopics topics)
    {
        _producer = accessor.GetProducer(nameof(IMessagePublisher));
        _propagator = Propagators.DefaultTextMapPropagator;
        _topics = topics;
    }

    public async Task<SampleMessage> PublishAsync(string payload, CancellationToken cancellationToken)
    {
        var message = new SampleMessage(Guid.NewGuid(), DateTimeOffset.UtcNow, payload);

        using var activity = Telemetry.ActivitySource.StartActivity("PublishMessage", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination", _topics.DemoTopic);
        activity?.SetTag("messaging.kafka.topic", _topics.DemoTopic);
        activity?.SetTag("messaging.message_id", message.Id);
        activity?.SetTag("messaging.message_payload_length", payload.Length);

        var headers = new Headers();
        var propagationContext = new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current);

        _propagator.Inject(propagationContext, headers, (carrier, key, value) =>
        {
            carrier.Add(key, Encoding.UTF8.GetBytes(value));
        });

        var value = JsonSerializer.Serialize(message);

        await _producer.ProduceAsync(
            _topics.DemoTopic,
            new Message<string, string>
            {
                Key = message.Id.ToString(),
                Value = value,
                Headers = headers
            },
            cancellationToken);

        Log.Information("Published message {Id}", message.Id);
        return message;
    }
}
