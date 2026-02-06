using System.Threading.Channels;
using Confluent.Kafka;
using AdvancedMessaging.Kafka.Services;

namespace AdvancedMessaging.Kafka.Consumers;

public class RawOptimizedConsumer : BackgroundService
{
    private readonly ILogger<RawOptimizedConsumer> _logger;
    private readonly BenchmarkMetrics _metrics;
    private readonly IConsumer<string, string> _consumer;
    private readonly Channel<ConsumeResult<string, string>> _channel;
    private const string Topic = "benchmark-topic";
    private const int WorkerCount = 10; 

    public RawOptimizedConsumer(ILogger<RawOptimizedConsumer> logger, BenchmarkMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
        
        _channel = Channel.CreateBounded<ConsumeResult<string, string>>(new BoundedChannelOptions(2000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = false
        });

        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "benchmark-group-optimized", // UNIQUE GROUP
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true, // Parallel commit is hard, accepting AutoCommit risk for speed benchmark
            EnableAutoOffsetStore = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workers = new List<Task>();
        for (int i = 0; i < WorkerCount; i++)
        {
            workers.Add(Task.Run(() => StartWorkerLoop(i, stoppingToken), stoppingToken));
        }

        await Task.Factory.StartNew(() => StartFetcherLoop(stoppingToken), TaskCreationOptions.LongRunning);
        await Task.WhenAll(workers);
    }

    private async Task StartFetcherLoop(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(Topic);
        
        while (!cancellationToken.IsCancellationRequested)
        {
             try
             {
                 var result = _consumer.Consume(cancellationToken);
                 await _channel.Writer.WriteAsync(result, cancellationToken);
             }
             catch (OperationCanceledException) { break; }
             catch (Exception) { /* ignore */ }
        }
        _channel.Writer.Complete();
        _consumer.Close();
    }

    private async Task StartWorkerLoop(int workerId, CancellationToken ct)
    {
        try
        {
            await foreach (var msg in _channel.Reader.ReadAllAsync(ct))
            {
                _metrics.Increment("RawOptimized");
            }
        }
        catch (Exception) { }
    }
}
