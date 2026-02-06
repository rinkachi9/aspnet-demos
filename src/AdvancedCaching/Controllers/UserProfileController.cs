using AdvancedCaching.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace AdvancedCaching.Controllers;

[ApiController]
[Route("users")]
public class UserProfileController(
    HybridCache hybridCache,
    ICacheInvalidator cacheInvalidator,
    ILogger<UserProfileController> logger) : ControllerBase
{
    // HybridCache handles stampede, L1+L2, serialization automatically.
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(int id, CancellationToken ct)
    {
        var cacheKey = $"user:{id}";

        var profile = await hybridCache.GetOrCreateAsync(
            cacheKey,
            async token => 
            {
                logger.LogInformation("Cache MISS for {Key}. Loading from 'Database'...", cacheKey);
                await Task.Delay(500, token); // Simulate DB latency
                return new UserProfile(id, $"User_{id}", DateTime.UtcNow);
            },
            cancellationToken: ct
        );

        return Ok(profile);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] string newName, CancellationToken ct)
    {
        logger.LogInformation("Updating User {Id} to {Name}", id, newName);

        // 1. Simulate DB Update
        // await db.SaveChangesAsync()...

        // 2. Invalidate Cache across ALL nodes
        var cacheKey = $"user:{id}";
        await cacheInvalidator.InvalidateAsync(cacheKey, ct);
        
        return NoContent();
    }
}

public record UserProfile(int Id, string Name, DateTime LastUpdated);
