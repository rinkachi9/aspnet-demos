namespace AdvancedDi.Services;

public interface IOrderProcessor
{
    void ProcessOrder(int orderId);
}

public class OrderProcessor(ILogger<OrderProcessor> logger) : IOrderProcessor
{
    public void ProcessOrder(int orderId)
    {
        logger.LogInformation("Processing Order {OrderId}...", orderId);
    }
}

public class LoggingOrderProcessorDecorator(IOrderProcessor inner, ILogger<LoggingOrderProcessorDecorator> logger) : IOrderProcessor
{
    public void ProcessOrder(int orderId)
    {
        logger.LogInformation("[Decorator] Before processing order {OrderId}", orderId);
        
        try
        {
            inner.ProcessOrder(orderId);
        }
        finally
        {
            logger.LogInformation("[Decorator] After processing order {OrderId}", orderId);
        }
    }
}
