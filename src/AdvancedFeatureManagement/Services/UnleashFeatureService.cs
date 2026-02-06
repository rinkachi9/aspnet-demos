using Unleash;
using Unleash.ClientFactory;

namespace AdvancedFeatureManagement.Services;

public interface IFeatureService
{
    bool IsEnabled(string toggleName);
    bool IsEnabled(string toggleName, UnleashContext context);
    void Dispose();
}

public class UnleashFeatureService : IFeatureService, IDisposable
{
    private readonly IUnleash _unleash;

    public UnleashFeatureService(IConfiguration configuration)
    {
        var settings = new UnleashSettings
        {
            AppName = "advanced-playground",
            Environment = "Development",
            UnleashApi = new Uri(configuration["Unleash:ApiUrl"] ?? "http://localhost:4242/api/"),
            CustomHttpHeaders = new Dictionary<string, string>
            {
                { "Authorization", configuration["Unleash:ApiKey"] ?? "*:development.unleash-insecure-api-token" }
            },
            FetchTogglesInterval = TimeSpan.FromSeconds(10) // Fast polling for demo
        };

        var factory = new UnleashClientFactory();
        _unleash = factory.CreateClient(settings, synchronousInitialization: true);
    }

    public bool IsEnabled(string toggleName) => _unleash.IsEnabled(toggleName);

    public bool IsEnabled(string toggleName, UnleashContext context) => _unleash.IsEnabled(toggleName, context);

    public void Dispose()
    {
        _unleash?.Dispose();
    }
}
