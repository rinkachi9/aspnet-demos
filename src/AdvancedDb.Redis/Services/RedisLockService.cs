using StackExchange.Redis;

namespace AdvancedDb.Redis.Services;

public class RedisLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisLockService> _logger;

    public RedisLockService(IConnectionMultiplexer redis, ILogger<RedisLockService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<bool> AcquireLockAsync(string resource, string lockId, TimeSpan expiry)
    {
        // SET key value NX PX expiry
        // NX = Only set if not exists (This is the lock acquisition)
        var result = await _db.StringSetAsync(resource, lockId, expiry, When.NotExists);
        if (result)
        {
            _logger.LogInformation("Lock acquired for {Resource} by {LockId}", resource, lockId);
        }
        else
        {
            _logger.LogWarning("Failed to acquire lock for {Resource} (Held by someone else)", resource);
        }
        return result;
    }

    public async Task ReleaseLockAsync(string resource, string lockId)
    {
        // CRITICAL: Lua Script to ensure we only delete if WE hold the lock
        // This prevents race condition where lock expires, someone else takes it, and we delete THEIR lock.
        var script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        var result = await _db.ScriptEvaluateAsync(script, new RedisKey[] { resource }, new RedisValue[] { lockId });
        
        if ((int)result == 1)
        {
            _logger.LogInformation("Lock released for {Resource}", resource);
        }
        else
        {
            _logger.LogWarning("Failed to release lock for {Resource} (Either expired or owner changed)", resource);
        }
    }
}
