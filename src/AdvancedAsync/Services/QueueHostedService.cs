using AdvancedAsync.Infrastructure;

namespace AdvancedAsync.Services;

public class QueueHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly QueueMetrics _metrics;
    private readonly ILogger<QueueHostedService> _logger;

    public QueueHostedService(
        IBackgroundTaskQueue taskQueue, 
        QueueMetrics metrics,
        ILogger<QueueHostedService> logger)
    {
        _taskQueue = taskQueue;
        _metrics = metrics;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queue Hosted Service is running.");

        await ProcessTaskQueueAsync(stoppingToken);
    }

    private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // DequeueAsync awaits until an item is available
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    // Execute the work item
                    await workItem(stoppingToken);
                    _metrics.TrackProcessed();
                }
                catch (Exception ex)
                {
                   _logger.LogError(ex, "Error occurred executing work item.");
                }
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred dequeuing.");
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queue Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
