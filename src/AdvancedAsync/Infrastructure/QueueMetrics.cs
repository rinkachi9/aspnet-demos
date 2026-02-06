using System.Diagnostics.Metrics;

namespace AdvancedAsync.Infrastructure;

public class QueueMetrics
{
    public const string MeterName = "AdvancedAsync.Queue";
    private readonly Meter _meter;
    private readonly UpDownCounter<long> _queueSize;
    private readonly Counter<long> _itemsAdded;
    private readonly Counter<long> _itemsProcessed;

    public QueueMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);
        _queueSize = _meter.CreateUpDownCounter<long>("queue_size", description: "Current number of items in the queue");
        _itemsAdded = _meter.CreateCounter<long>("items_added", description: "Total number of items added to the queue");
        _itemsProcessed = _meter.CreateCounter<long>("items_processed", description: "Total number of items processed");
    }

    public void TrackAdded()
    {
        _itemsAdded.Add(1);
        _queueSize.Add(1);
    }

    public void TrackProcessed()
    {
        _itemsProcessed.Add(1);
        _queueSize.Add(-1);
    }
}
