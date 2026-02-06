namespace AdvancedMessaging.Kafka.Services;

public class SimulationWorker(RawKafkaProducer producer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for Kafka to be ready roughly
        await Task.Delay(5000, stoppingToken);

        var orderId = "ORD-123";
        // Produce sequence of events
        await producer.ProduceOrderAsync(orderId, "Created");
        await producer.ProduceOrderAsync(orderId, "Paid");
        await producer.ProduceOrderAsync(orderId, "Shipped");
        
        // This ensures they land in the same partition and are consumed in order.
    }
}
