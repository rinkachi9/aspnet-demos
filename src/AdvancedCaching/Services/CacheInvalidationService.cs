using Microsoft.Extensions.Caching.Hybrid;
using StackExchange.Redis;

namespace AdvancedCaching.Services;

public class CacheInvalidationService(
    IConnectionMultiplexer connectionMultiplexer,
    HybridCache hybridCache,
    ILogger<CacheInvalidationService> logger)
    : BackgroundService
{
    private const string ChannelName = "cache-invalidation";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = connectionMultiplexer.GetSubscriber();

        await subscriber.SubscribeAsync(RedisChannel.Literal(ChannelName), (channel, value) =>
        {
            var key = value.ToString();
            logger.LogInformation("Received invalidation for key: {Key}. Removing from local HybridCache.", key);

            // Evict from local L1
            // Note: HybridCache.RemoveAsync propagates to L2 (Distributed) as well usually, 
            // but here the goal is ensuring THIS node's L1 is cleared if it was stale.
            // Since we use a central L2 (Redis), calling RemoveAsync is fine/correct.
            // Ideally we'd valid to ONLY remove L1, but HybridCache API abstracts that.
            // The important part is: local L1 must die.
            
            // Fire and forget in the callback context, but we should be careful.
            // We use the stoppingToken from the service, but inside the callback it might be tricky.
            // For safety, we just launch the task.
            _ = RemoveSafeAsync(key, stoppingToken);
        });
        
        // Keep alive
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task RemoveSafeAsync(string key, CancellationToken token)
    {
        try
        {
            await hybridCache.RemoveAsync(key, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing key {Key} from HybridCache", key);
        }
    }
}
