using Unleash;
using Unleash.ClientFactory;

namespace AdvancedFeatureFlags.Services;

public interface IUnleashService
{
    bool IsEnabled(string toggleName);
}

public class UnleashService : IUnleashService, IDisposable
{
    private readonly IUnleash _unleash;

    public UnleashService(IConfiguration config)
    {
        var settings = new UnleashSettings
        {
            AppName = config["Unleash:AppName"],
            InstanceTag = config["Unleash:InstanceTag"],
            UnleashApi = new Uri(config["Unleash:UnleashApi"]!),
            FetchTogglesInterval = TimeSpan.Parse(config["Unleash:FetchTogglesInterval"]!.Replace("s", ""))
        };

        // In real world, use an API Token (Authorization header)
        // For open source demo setup without auth enabled, this is enough
        
        var factory = new UnleashClientFactory();
        _unleash = factory.CreateClient(settings, synchronousInitialization: true);
    }

    public bool IsEnabled(string toggleName) => _unleash.IsEnabled(toggleName);

    public void Dispose()
    {
        _unleash?.Dispose();
    }
}
