using System.Threading.Channels;

namespace AdvancedAsync.Infrastructure;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _queue;
    private readonly QueueMetrics _metrics;
    private readonly ILogger<BackgroundTaskQueue> _logger;

    public BackgroundTaskQueue(QueueMetrics metrics, ILogger<BackgroundTaskQueue> logger, IConfiguration config)
    {
        _metrics = metrics;
        _logger = logger;
        
        // Configuration options
        int capacity = config.GetValue<int>("QueueCapacity", 100);

        // Bounded Channel for Backpressure
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait // CRITICAL: This enables Backpressure. Producer awaits if full.
        };
        
        _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
        
        _logger.LogInformation("Queue initialized with Capacity: {Capacity}, FullMode: Wait", capacity);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
    {
        if (workItem == null) throw new ArgumentNullException(nameof(workItem));

        // WriteAsync will await if the channel is full (Backpressure)
        await _queue.Writer.WriteAsync(workItem);
        
        _metrics.TrackAdded();
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
