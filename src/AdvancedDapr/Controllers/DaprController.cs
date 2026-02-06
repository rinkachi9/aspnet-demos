using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDapr.Controllers;

[ApiController]
[Route("[controller]")]
public class DaprController : ControllerBase
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<DaprController> _logger;

    public DaprController(DaprClient daprClient, ILogger<DaprController> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    // 1. Pub/Sub Subscription
    [Topic("pubsub", "orders")]
    [HttpPost("orders")]
    public async Task<IActionResult> OrderSubscriber([FromBody] object orderData)
    {
        _logger.LogInformation("Received Order Event via PubSub: {@OrderData}", orderData);
        await Task.CompletedTask;
        return Ok();
    }

    // 2. Publish Event (Helper endpoint to trigger pubsub)
    [HttpPost("publish")]
    public async Task<IActionResult> PublishOrder([FromBody] object orderData)
    {
        await _daprClient.PublishEventAsync("pubsub", "orders", orderData);
        return Ok("Published");
    }

    // 3. Service Invocation (Invoking self for demo)
    [HttpGet("invoke-self")]
    public async Task<IActionResult> InvokeSelf()
    {
        // Invoking "dapr-app-id" service method "weatherforecast" (just an example, calling root here)
        // Ensure app-id is set when running via dapr
        try
        {
            var result = await _daprClient.InvokeMethodAsync<string>(HttpMethod.Get, "advanced-dapr", ""); 
            return Ok($"Invoked Self Result: {result}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Invocation Failed: {ex.Message}");
        }
    }

    // 4. State Management
    [HttpPost("state/{key}")]
    public async Task<IActionResult> SaveState(string key, [FromBody] string value)
    {
        await _daprClient.SaveStateAsync("statestore", key, value);
        return Ok();
    }

    [HttpGet("state/{key}")]
    public async Task<IActionResult> GetState(string key)
    {
        var value = await _daprClient.GetStateAsync<string>("statestore", key);
        return Ok(value);
    }
}
