using StackExchange.Redis;

namespace AdvancedCaching.Services;

public class RedisCacheInvalidator(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheInvalidator> logger)
    : ICacheInvalidator
{
    private const string ChannelName = "cache-invalidation";

    public async Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        var subscriber = connectionMultiplexer.GetSubscriber();

        // Publish the key to all nodes
        await subscriber.PublishAsync(RedisChannel.Literal(ChannelName), new RedisValue(key));

        logger.LogInformation("Published invalidation for key: {Key}", key);
    }
}
