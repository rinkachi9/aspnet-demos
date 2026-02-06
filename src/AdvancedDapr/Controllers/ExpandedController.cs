using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDapr.Controllers;

[ApiController]
[Route("[controller]")]
public class ExpandedController : ControllerBase
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<ExpandedController> _logger;

    public ExpandedController(DaprClient daprClient, ILogger<ExpandedController> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    // 1. Secrets Management
    [HttpGet("secrets/{key}")]
    public async Task<IActionResult> GetSecret(string key)
    {
        try
        {
            var secret = await _daprClient.GetSecretAsync("secrets", key);
            return Ok(new { Key = key, Value = secret[key] });
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    // 2. Distributed Lock
    [HttpPost("lock/{resourceId}")]
    public async Task<IActionResult> AcquireLock(string resourceId)
    {
        // Must use a state store that supports locking (e.g. Redis)
        const string storeName = "statestore";
        var lockKey = $"lock:{resourceId}";
        var ownerId = Guid.NewGuid().ToString();

        await using var handle = await _daprClient.Lock(storeName, lockKey, ownerId, 10);
        
        if (handle.Success)
        {
            _logger.LogInformation("Lock acquired for {Resource} by {Owner}", resourceId, ownerId);
            
            // Critical Section Simulation
            await Task.Delay(2000); 
            
            _logger.LogInformation("Job done, releasing lock...");
            // Lock is released when 'handle' is disposed or explicity unlocked
            return Ok($"Locked and processed {resourceId}");
        }
        else
        {
            return StatusCode(423, $"Could not acquire lock for {resourceId}");
        }
    }
}
