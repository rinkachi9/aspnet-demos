using AdvancedFeatureFlags.Services;

namespace AdvancedFeatureFlags.Workers;

public class DynamicWorker : BackgroundService
{
    private readonly IUnleashService _unleash;
    private readonly ILogger<DynamicWorker> _logger;

    public DynamicWorker(IUnleashService unleash, ILogger<DynamicWorker> logger)
    {
        _unleash = unleash;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dynamic Worker Started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Check flag every iteration - Unleash Client caches it locally and refreshes in background
            // so this IsEnabled call is effectively O(1) memory lookup (FAST).
            
            if (_unleash.IsEnabled("worker-maintenance-mode"))
            {
                _logger.LogWarning("[MAINTENANCE] Worker paused via Feature Flag...");
                await Task.Delay(5000, stoppingToken);
                continue;
            }

            if (_unleash.IsEnabled("worker-v2-logic"))
            {
                _logger.LogInformation("[V2] Processing using NEW Optimized Algorithm 🚀");
            }
            else
            {
                _logger.LogInformation("[V1] Processing using Standard Algorithm 🐢");
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}
