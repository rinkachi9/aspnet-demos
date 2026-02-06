using DotNetCore.CAP;

namespace AdvancedCAP.Subscribers;

public class PaymentSubscriber : ICapSubscribe
{
    private readonly ILogger<PaymentSubscriber> _logger;
    private readonly ICapPublisher _publisher;

    public PaymentSubscriber(ILogger<PaymentSubscriber> logger, ICapPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    [CapSubscribe("order.created")]
    public async Task ProcessPayment(dynamic message)
    {
        var orderId = (string)message.Id;
        _logger.LogInformation("[PaymentService] Processing payment for Order {OrderId}...", orderId);

        // Simulate processing time
        await Task.Delay(500);

        // Simulate 80% Success Rate
        bool paymentSuccess = Random.Shared.Next(10) > 2;

        if (paymentSuccess)
        {
            _logger.LogInformation("[PaymentService] Payment Successful for {OrderId}", orderId);
            await _publisher.PublishAsync("payment.succeeded", new { OrderId = orderId });
        }
        else
        {
            _logger.LogError("[PaymentService] Payment FAILED for {OrderId}", orderId);
            await _publisher.PublishAsync("payment.failed", new { OrderId = orderId, Reason = "Insufficient Funds" });
        }
    }
}
