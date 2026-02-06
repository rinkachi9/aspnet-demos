using StackExchange.Redis;

namespace AdvancedDb.Redis.Services;

public class StreamConsumerService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<StreamConsumerService> _logger;
    private const string StreamName = "benchmark-stream";
    private const string GroupName = "consumer-group-1";
    private const string ConsumerName = "worker-1";

    public StreamConsumerService(IConnectionMultiplexer redis, ILogger<StreamConsumerService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        try
        {
            // Ensure Group Exists (MKSTREAM creates stream if empty)
            await db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0-0", true);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP")) { }
        
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _redis.GetDatabase();
        while (!stoppingToken.IsCancellationRequested)
        {
            // XREADGROUP
            var results = await db.StreamReadGroupAsync(
                StreamName, 
                GroupName, 
                ConsumerName, 
                ">", // Never delivered messages
                count: 1); // Process 1 by 1 for simplicity in demo

            if (results.Length > 0)
            {
                foreach (var entry in results)
                {
                    _logger.LogInformation("Received Stream Msg: {Id} - {Body}", entry.Id, entry["message"]);
                    
                    // Simulate work
                    await Task.Delay(50);
                    
                    // XACK
                    await db.StreamAcknowledgeAsync(StreamName, GroupName, entry.Id);
                }
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }
}
