using StackExchange.Redis;

namespace AdvancedDb.Redis.Services;

public class RateLimiter
{
    private readonly IDatabase _db;

    public RateLimiter(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<bool> IsAllowedAsync(string userId, int limit, TimeSpan period)
    {
        var key = $"ratelimit:{userId}";
        
        // LUA Script that Atomically:
        // 1. Increments counter
        // 2. Sets expiry IF logic dictates (first request)
        // 3. Returns current count
        var script = @"
            var current = redis.call('incr', KEYS[1])
            if tonumber(current) == 1 then
                redis.call('pexpire', KEYS[1], ARGV[1])
            end
            return current";

        var countResult = await _db.ScriptEvaluateAsync(script, 
            new RedisKey[] { key }, 
            new RedisValue[] { period.TotalMilliseconds });

        return (long)countResult <= limit;
    }
}
