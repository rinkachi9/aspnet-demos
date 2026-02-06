using StackExchange.Redis;

namespace AdvancedDb.Redis.Services;

public class BulkService
{
    private readonly IDatabase _db;
    private readonly ILogger<BulkService> _logger;

    public BulkService(IConnectionMultiplexer redis, ILogger<BulkService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task RunPerformanceTestAsync(int count)
    {
        _logger.LogInformation("Starting Pipelining Test with {Count} ops...", count);

        // 1. SEQUENTIAL (Bad)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < count; i++)
        {
            await _db.StringSetAsync($"seq:{i}", i);
        }
        sw.Stop();
        var seqTime = sw.ElapsedMilliseconds;

        // 2. PIPELINING (Good)
        sw.Restart();
        var batch = _db.CreateBatch();
        var tasks = new List<Task>();
        for (int i = 0; i < count; i++)
        {
             tasks.Add(batch.StringSetAsync($"pipe:{i}", i)); // Fire and forget into buffer
        }
        batch.Execute(); // Send all at once
        await Task.WhenAll(tasks); // Wait for all replies
        sw.Stop();
        var pipeTime = sw.ElapsedMilliseconds;

        _logger.LogWarning("PERFORMANCE: Sequential: {Seq}ms | Pipelining: {Pipe}ms | Improvement: {Ratio}x", 
            seqTime, pipeTime, (double)seqTime/pipeTime);
    }
}
