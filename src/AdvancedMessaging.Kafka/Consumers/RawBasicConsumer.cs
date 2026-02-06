using Confluent.Kafka;
using AdvancedMessaging.Kafka.Services;

namespace AdvancedMessaging.Kafka.Consumers;

public class RawBasicConsumer : BackgroundService
{
    private readonly ILogger<RawBasicConsumer> _logger;
    private readonly BenchmarkMetrics _metrics;
    private readonly IConsumer<string, string> _consumer;
    private const string Topic = "benchmark-topic";

    public RawBasicConsumer(ILogger<RawBasicConsumer> logger, BenchmarkMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "benchmark-group-basic", // UNIQUE GROUP
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false 
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(() => StartConsumerLoop(stoppingToken), TaskCreationOptions.LongRunning);
    }

    private void StartConsumerLoop(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(Topic);
        
        while (!cancellationToken.IsCancellationRequested)
        {
             try
             {
                 var result = _consumer.Consume(cancellationToken);
                 
                 // PROCESSING
                 _metrics.Increment("RawBasic");
                 
                 _consumer.Commit(result);
             }
             catch (OperationCanceledException) { break; }
             catch (Exception e)
             {
                 _logger.LogError(e, "Error consuming");
             }
        }
        
        _consumer.Close();
    }
}
