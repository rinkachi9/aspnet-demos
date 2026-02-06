using System.Threading.Channels;
using AdvancedServicePatterns.Models;

namespace AdvancedServicePatterns.Background;

public interface ITaskQueue
{
    ValueTask EnqueueAsync(ReportRequest item);
    ValueTask<ReportRequest> DequeueAsync(CancellationToken cancellationToken);
}

public class ChannelQueue : ITaskQueue
{
    private readonly Channel<ReportRequest> _channel;

    public ChannelQueue()
    {
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<ReportRequest>(options);
    }

    public async ValueTask EnqueueAsync(ReportRequest item)
    {
        await _channel.Writer.WriteAsync(item);
    }

    public async ValueTask<ReportRequest> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}
