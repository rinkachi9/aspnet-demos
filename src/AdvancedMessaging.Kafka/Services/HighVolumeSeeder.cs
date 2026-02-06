namespace AdvancedMessaging.Kafka.Services;

public class HighVolumeSeeder
{
    private readonly PerformanceKafkaProducer _fastProducer;
    private readonly ReliableKafkaProducer _safeProducer;
    private readonly ILogger<HighVolumeSeeder> _logger;

    public HighVolumeSeeder(
        PerformanceKafkaProducer fastProducer,
        ReliableKafkaProducer safeProducer,
        ILogger<HighVolumeSeeder> logger)
    {
        _fastProducer = fastProducer;
        _safeProducer = safeProducer;
        _logger = logger;
    }

    public async Task SeedAsync(int count, bool reliableMode, CancellationToken ct)
    {
        var modeName = reliableMode ? "RELIABLE" : "PERFORMANCE";
        _logger.LogInformation("Starting SEED [{Mode}] of {Count} messages...", modeName, count);
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var chunks = 1000;
        var pending = new List<Task>();

        for (int i = 0; i < count; i++)
        {
            var key = $"Key-{i % 100}";
            var val = $"Payload-{i}-{Guid.NewGuid()}";

            if (reliableMode)
            {
                pending.Add(_safeProducer.ProduceAsync(key, val));
            }
            else
            {
                pending.Add(_fastProducer.ProduceAsync(key, val));
            }

            if (pending.Count >= chunks)
            {
                await Task.WhenAll(pending);
                pending.Clear();
                if (i % 10000 == 0) _logger.LogInformation("Seeded {Num}...", i);
            }
        }
        
        await Task.WhenAll(pending);
        
        if (reliableMode) _safeProducer.Flush();
        else _fastProducer.Flush();

        sw.Stop();
        _logger.LogInformation("SEED COMPLETE [{Mode}]. {Count} in {Elapsed}ms ({Rate:F0} msg/s)", 
            modeName, count, sw.ElapsedMilliseconds, count / sw.Elapsed.TotalSeconds);
    }
}
