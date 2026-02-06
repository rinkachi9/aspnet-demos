using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDapr.Controllers;

[ApiController]
[Route("[controller]")]
public class BindingsController : ControllerBase
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<BindingsController> _logger;

    public BindingsController(DaprClient daprClient, ILogger<BindingsController> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    // 1. Input Bond (Cron)
    // Dapr calls POST /cron when the binding triggers
    [HttpPost("/cron")]
    public async Task<IActionResult> OnCronTrigger()
    {
        _logger.LogInformation("Cron Binding Triggered at {Time}", DateTime.UtcNow);

        // 2. Output Binding (File)
        // Write a file using the Dapr output binding
        var content = new { message = "Scheduled task executed", time = DateTime.UtcNow };
        await _daprClient.InvokeBindingAsync("files", "create", content);
        
        return Ok();
    }
}
