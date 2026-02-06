namespace ResiliencePatterns.Services;

public interface IExternalWeatherService
{
    Task<string> GetCurrentWeatherAsync(string city);
}

public class ExternalWeatherService : IExternalWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalWeatherService> _logger;

    public ExternalWeatherService(HttpClient httpClient, ILogger<ExternalWeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetCurrentWeatherAsync(string city)
    {
        _logger.LogInformation("Calling External API for {City}...", city);
        
        // Simulation of unstable external service
        var random = Random.Shared.Next(1, 11);
        
        // 30% chance of failure
        if (random <= 3) 
        {
            _logger.LogError("External API Failed (Simulated)!");
            throw new HttpRequestException("Simulated Network Failure");
        }

        // Simulate network latency
        await Task.Delay(200);

        return $"Weather in {city}: {random * 2 + 10}°C, Sunny";
    }
}
