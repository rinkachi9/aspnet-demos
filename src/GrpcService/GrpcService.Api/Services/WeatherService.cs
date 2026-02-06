using Grpc.Core;

namespace GrpcService.Api.Services;

public class WeatherService : Weather.WeatherBase
{
    private readonly ILogger<WeatherService> _logger;
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherService(ILogger<WeatherService> logger)
    {
        _logger = logger;
    }

    public override Task<WeatherReply> GetWeather(WeatherRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Unary: Getting weather for {City}", request.City);
        
        var reply = new WeatherReply
        {
            City = request.City,
            TemperatureC = Random.Shared.Next(-20, 55),
            Description = Summaries[Random.Shared.Next(Summaries.Length)],
            Timestamp = DateTime.UtcNow.ToString("O")
        };
        
        return Task.FromResult(reply);
    }

    public override async Task MonitorWeather(WeatherRequest request, IServerStreamWriter<WeatherReply> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("Streaming: Monitoring weather for {City}", request.City);

        // Stream 5 updates
        for (int i = 0; i < 5; i++)
        {
            if (context.CancellationToken.IsCancellationRequested) break;

            var reply = new WeatherReply
            {
                City = request.City,
                TemperatureC = Random.Shared.Next(-20, 55),
                Description = Summaries[Random.Shared.Next(Summaries.Length)],
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            await responseStream.WriteAsync(reply);
            
            // Simulating real-time updates
            await Task.Delay(1000); 
        }
        
        _logger.LogInformation("Streaming: Completed for {City}", request.City);
    }
}
