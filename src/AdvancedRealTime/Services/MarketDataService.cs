using Grpc.Core;

namespace AdvancedRealTime.Services;

public class MarketDataService : MarketData.MarketDataBase
{
    private readonly ILogger<MarketDataService> _logger;

    public MarketDataService(ILogger<MarketDataService> logger)
    {
        _logger = logger;
    }

    public override async Task SubscribeToTicker(TickerRequest request, IServerStreamWriter<MarketUpdate> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("Client subscribed to {Symbol}", request.Symbol);

        var random = new Random();
        double price = 100.0;

        // Keep streaming until client disconnects
        while (!context.CancellationToken.IsCancellationRequested)
        {
            // Simulate price movement
            price += (random.NextDouble() * 2.0) - 1.0; 

            var update = new MarketUpdate
            {
                Symbol = request.Symbol,
                Price = Math.Round(price, 2),
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            await responseStream.WriteAsync(update);
            
            // Artificial delay (high frequency trading simulation :D)
            await Task.Delay(500); 
        }

        _logger.LogInformation("Client unsubscribed from {Symbol}", request.Symbol);
    }
}
