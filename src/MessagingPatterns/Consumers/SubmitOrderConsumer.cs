using MassTransit;
using MessagingPatterns.Contracts;

namespace MessagingPatterns.Consumers;

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    private readonly ILogger<SubmitOrderConsumer> _logger;

    public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        _logger.LogInformation("Received SubmitOrder: {OrderId} from {CustomerNumber}", 
            context.Message.OrderId, context.Message.CustomerNumber);

        // Simulate processing
        await Task.Delay(500);

        // Publish Event (Pub/Sub pattern)
        await context.Publish(new OrderAccepted(context.Message.OrderId, DateTime.UtcNow));
        
        _logger.LogInformation("OrderAccepted published for {OrderId}", context.Message.OrderId);
    }
}
