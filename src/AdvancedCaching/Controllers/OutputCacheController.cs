using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AdvancedCaching.Controllers;

[ApiController]
[Route("output-cache")]
public class OutputCacheController(ILogger<OutputCacheController> logger) : ControllerBase
{
    // Scenario 1: Basic Time-based Expiration
    [HttpGet("time")]
    [OutputCache(Duration = 10)] // Caches for 10s
    public IActionResult GetTime()
    {
        logger.LogInformation("Executing GetTime (Not Cached)");
        return Ok(new { Time = DateTime.UtcNow });
    }

    // Scenario 2: Vary By Query Parameter
    [HttpGet("vary-query")]
    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "culture", "page" })]
    public IActionResult GetVaryByQuery([FromQuery] string culture, [FromQuery] int page)
    {
        logger.LogInformation("Executing GetVaryByQuery culture={Culture}, page={Page}", culture, page);
        return Ok(new { Culture = culture, Page = page, Time = DateTime.UtcNow });
    }

    // Scenario 3: Vary By Header
    [HttpGet("vary-header")]
    [OutputCache(Duration = 60, VaryByHeaderNames = new[] { "X-Client-Type" })]
    public IActionResult GetVaryByHeader()
    {
        var clientType = Request.Headers["X-Client-Type"].ToString();
        logger.LogInformation("Executing GetVaryByHeader Client={Client}", clientType);
        return Ok(new { Client = clientType, Time = DateTime.UtcNow });
    }

    // Scenario 4: Tagging and Eviction
    [HttpGet("tagged")]
    [OutputCache(PolicyName = "Expire20")] // Defined in Extensions (Tags: "articles")
    public IActionResult GetTagged()
    {
        logger.LogInformation("Executing GetTagged");
        return Ok(new { Data = "Some Article Data", Time = DateTime.UtcNow });
    }

    [HttpPost("evict")]
    public async Task<IActionResult> EvictTag([FromServices] IOutputCacheStore store)
    {
        // In .NET 8, we inject IOutputCacheStore to evict.
        // We defined "articles" tag in Extensions.
        await store.EvictByTagAsync("articles", CancellationToken.None);
        return Ok("Evicted tag 'articles'");
    }
}
