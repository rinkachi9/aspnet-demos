using System.Collections.Concurrent;

namespace AdvancedMedia.Services;

public class MediaBackgroundProcessor : BackgroundService
{
    private readonly ILogger<MediaBackgroundProcessor> _logger;
    // In real app, use a queue (RabbitMQ/Azure Queue)
    // For demo, we use a static concurrent queue
    public static readonly ConcurrentQueue<string> ProcessingQueue = new();

    public MediaBackgroundProcessor(ILogger<MediaBackgroundProcessor> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (ProcessingQueue.TryDequeue(out var fileName))
            {
                _logger.LogInformation("Starting transcoding for {FileName}...", fileName);
                
                // Simulate CPU-intensive work
                await Task.Delay(5000, stoppingToken);
                
                _logger.LogInformation("Transcoding finished for {FileName}. Metadata updated.", fileName);
            }
            else
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
