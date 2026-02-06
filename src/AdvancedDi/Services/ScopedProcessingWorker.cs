namespace AdvancedDi.Services;

public interface IScopedProcessingService
{
    Task DoWorkAsync(CancellationToken stoppingToken);
}

public class DefaultScopedProcessingService(ILogger<DefaultScopedProcessingService> logger) : IScopedProcessingService
{
    private readonly Guid _executionId = Guid.NewGuid();

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Checked Scoped Service (ID: {Id}). Doing work...", _executionId);
        await Task.Delay(100, stoppingToken);
    }
}

// The Worker (Singleton) consuming a Scoped Service
public class ScopedProcessingWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ScopedProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Scoped Processing Worker Service is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker creating a new scope...");

            // CreateAsyncScope is preferred for IAsyncDisposable support
            await using (var scope = scopeFactory.CreateAsyncScope())
            {
                var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();
                await scopedProcessingService.DoWorkAsync(stoppingToken);
            }

            // Simulate partial work cycle
            await Task.Delay(5000, stoppingToken);
        }
    }
}
