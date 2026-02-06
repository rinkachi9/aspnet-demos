using Grpc.Core;

namespace AdvancedRealTime.Services;

public class TradingService : Trading.TradingBase
{
    private readonly ILogger<TradingService> _logger;

    public TradingService(ILogger<TradingService> logger)
    {
        _logger = logger;
    }

    public override async Task LiveTrading(IAsyncStreamReader<TradeOrder> requestStream, IServerStreamWriter<TradeUpdate> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("Connected to Live Trading Floor");

        var readTask = Task.Run(async () =>
        {
            await foreach (var order in requestStream.ReadAllAsync())
            {
                _logger.LogInformation("Received Order: {Side} {Quantity} {Symbol} @ {Price}", order.Side, order.Quantity, order.Symbol, order.Price);

                // Simulate processing
                await Task.Delay(100); 

                await responseStream.WriteAsync(new TradeUpdate
                {
                    OrderId = Guid.NewGuid().ToString(),
                    Status = "FILLED",
                    Message = $"Executed {order.Quantity} @ {order.Price}"
                });
            }
        });

        // Keep connection alive for updates independent of requests (e.g. market events)
        while (!readTask.IsCompleted)
        {
            await Task.Delay(2000);
            await responseStream.WriteAsync(new TradeUpdate
            {
                OrderId = "SYSTEM",
                Status = "INFO",
                Message = "Market is open"
            });
        }

        await readTask;
    }
}
