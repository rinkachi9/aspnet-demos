using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace AdvancedCaching.Controllers;

[ApiController]
[Route("distributed")]
public class DistributedController(
    IDistributedCache distCache,
    IConnectionMultiplexer redis,
    ILogger<DistributedController> logger) : ControllerBase
{
    // Scenario 1: Manual Distributed Cache
    [HttpGet("cache/{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var value = await distCache.GetStringAsync(key);
        if (value == null)
        {
            return NotFound("Key not found");
        }
        return Ok(value);
    }

    [HttpPost("cache/{key}")]
    public async Task<IActionResult> Set(string key, [FromBody] string value)
    {
        await distCache.SetStringAsync(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        });
        return Ok($"Set {key}={value}");
    }

    // Scenario 2: Distributed Lock (Redlock simulation via SET NX)
    // For Production: Use RedLock.net library for proper consensus algorithm.
    // This is a simple implementation showcasing the concept.
    [HttpPost("lock/{resource}")]
    public async Task<IActionResult> DoCriticalWork(string resource)
    {
        var db = redis.GetDatabase();
        var lockKey = $"lock:{resource}";
        var token = Guid.NewGuid().ToString();

        // Try to acquire lock: Set key only if not exists (NX), expire in 10s
        if (await db.LockTakeAsync(lockKey, token, TimeSpan.FromSeconds(10)))
        {
            try
            {
                logger.LogInformation("Acquired lock for {Resource}", resource);
                
                // Simulate critical section
                await Task.Delay(2000); 

                return Ok(new { status = "Work Done", token });
            }
            finally
            {
                // Release lock
                await db.LockReleaseAsync(lockKey, token);
                logger.LogInformation("Released lock for {Resource}", resource);
            }
        }
        else
        {
            return StatusCode(429, "Resource is locked by another process. Try again later.");
        }
    }
}
