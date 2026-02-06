using Confluent.Kafka;

namespace AdvancedMessaging.Kafka.Services;

public class RawKafkaProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<RawKafkaProducer> _logger;
    private const string Topic = "orders";

    public RawKafkaProducer(ILogger<RawKafkaProducer> logger)
    {
        _logger = logger;
        
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            // Idempotence ensures exactly-once delivery semantics at the producer level
            EnableIdempotence = true, 
            Acks = Acks.All // Robustness: Wait for all replicas
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceOrderAsync(string orderId, string status)
    {
        try
        {
            // PARTITIONING: By using 'Key', we ensure all events for OrderId go to the same partition.
            // This guarantees strict ordering for that specific order.
            var result = await _producer.ProduceAsync(Topic, new Message<string, string> 
            { 
                Key = orderId, 
                Value = $"Order {orderId} is now {status}" 
            });

            _logger.LogInformation($"Produced: {result.Value} to Partition: {result.Partition}");
        }
        catch (ProduceException<string, string> e)
        {
            _logger.LogError($"Delivery failed: {e.Error.Reason}");
        }
    }
}
