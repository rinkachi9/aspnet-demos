using System.Collections.Concurrent;

namespace AdvancedMessaging.Kafka.Services;

public class BenchmarkMetrics
{
    private readonly ConcurrentDictionary<string, int> _counters = new();
    private readonly ILogger<BenchmarkMetrics> _logger;

    public BenchmarkMetrics(ILogger<BenchmarkMetrics> logger)
    {
        _logger = logger;
        // Start reporting loop
        Task.Run(ReportLoop);
    }

    public void Increment(string consumerName)
    {
        _counters.AddOrUpdate(consumerName, 1, (_, current) => current + 1);
    }

    private async Task ReportLoop()
    {
        var lastSnapshot = new Dictionary<string, int>();
        
        while (true)
        {
            await Task.Delay(1000);
            
            var currentSnapshot = new Dictionary<string, int>(_counters);
            var report = new List<string>();

            foreach (var kvp in currentSnapshot)
            {
                var name = kvp.Key;
                var total = kvp.Value;
                var last = lastSnapshot.GetValueOrDefault(name, 0);
                var delta = total - last;
                
                report.Add($"{name}: {delta}/s (Total: {total})");
            }
            
            if (report.Any(x => !x.Contains("0/s"))) // Only log if activity
            {
                _logger.LogWarning("[BENCHMARK] " + string.Join(" | ", report));
            }

            lastSnapshot = currentSnapshot;
        }
    }
}
