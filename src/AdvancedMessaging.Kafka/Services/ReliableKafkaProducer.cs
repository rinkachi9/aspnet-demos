using Confluent.Kafka;

namespace AdvancedMessaging.Kafka.Services;

public class ReliableKafkaProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<ReliableKafkaProducer> _logger;
    private const string Topic = "benchmark-topic";

    public ReliableKafkaProducer(ILogger<ReliableKafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            // RELIABILITY SETTINGS
            Acks = Acks.All, // Wait for all ISR to ack
            EnableIdempotence = true, // Prevent dupes on retry
            MessageSendMaxRetries = int.MaxValue,
            MaxInFlight = 5 // Required for Idempotence
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync(string key, string value)
    {
        await _producer.ProduceAsync(Topic, new Message<string, string> { Key = key, Value = value });
    }
    
    public void Flush() => _producer.Flush(TimeSpan.FromSeconds(10));
}
