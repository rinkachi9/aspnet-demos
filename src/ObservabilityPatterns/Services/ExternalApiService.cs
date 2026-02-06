using System.Diagnostics;

namespace ObservabilityPatterns.Services;

public class ExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiService> _logger;

    public ExternalApiService(HttpClient httpClient, ILogger<ExternalApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetCurrentWeatherAsync()
    {
        // Activity tags can be added manually to the current span (which OTel creates automatically for HttpClient)
        Activity.Current?.SetTag("api.target", "OpenMeteo");

        try
        {
            // Calling a public free API
            var response = await _httpClient.GetAsync("https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current_weather=true");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully fetched weather data: {Length} bytes", content.Length);
            
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch weather data");
            throw;
        }
    }
}
