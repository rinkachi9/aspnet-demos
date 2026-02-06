using DotNetCore.CAP;

namespace AdvancedCAP.Subscribers;

public class OrderSubscriber : ICapSubscribe
{
    private readonly ILogger<OrderSubscriber> _logger;

    public OrderSubscriber(ILogger<OrderSubscriber> logger)
    {
        _logger = logger;
    }

    [CapSubscribe("order.created")]
    public void HandleOrderCreated(dynamic message)
    {
        // "message" comes from JSON deserialization
        // In real world, use a typed class or record
        _logger.LogInformation("Received Order Created Event! ID: {OrderId}, Product: {Product}", 
            (string)message.Id, 
            (string)message.Product);
        
        // Simulating processing logic...
        // If Exception is thrown here, CAP retries automatically with backoff.
    }
}
