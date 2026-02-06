using Confluent.Kafka;

namespace AdvancedMessaging.Kafka.Services;

public class PerformanceKafkaProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<PerformanceKafkaProducer> _logger;
    private const string Topic = "benchmark-topic";

    public PerformanceKafkaProducer(ILogger<PerformanceKafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            // PERFORMANCE SETTINGS
            Acks = Acks.None, // Fire and forget. Don't wait for broker.
            LingerMs = 100, // Aggregate messages for up to 100ms
            BatchNumMessages = 100000, 
            CompressionType = CompressionType.Lz4, // Fast compression
            QueueBufferingMaxMessages = 1000000
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync(string key, string value)
    {
        // For max testing, we don't even wait for result usually, but here await ensures buffer isn't full
        await _producer.ProduceAsync(Topic, new Message<string, string> { Key = key, Value = value });
    }
    
    public void Flush() => _producer.Flush(TimeSpan.FromSeconds(10));
}
