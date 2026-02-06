using Microsoft.AspNetCore.Mvc;

namespace AdvancedLogging.Controllers;

[ApiController]
[Route("[controller]")]
public class LoggingDemoController : ControllerBase
{
    private readonly ILogger<LoggingDemoController> _logger;

    public LoggingDemoController(ILogger<LoggingDemoController> logger)
    {
        _logger = logger;
    }

    [HttpGet("standard")]
    public IActionResult GetStandard()
    {
        _logger.LogInformation("This is a standard informational log.");
        _logger.LogWarning("This is a warning with a random ID: {RandomId}", Guid.NewGuid());
        return Ok("Logged");
    }

    [HttpGet("scope")]
    public IActionResult GetScoped()
    {
        // BeginScope adds properties to all logs within the block
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["TransactionId"] = Guid.NewGuid(),
            ["ScopeName"] = "ImportantBusinessTransaction"
        }))
        {
            _logger.LogInformation("Starting transaction...");
            
            // Nested method calls will inherit these properties
            ProcessStep1();
            
            _logger.LogInformation("Transaction finished.");
        }
        return Ok("Scoped Logs Generated");
    }

    private void ProcessStep1()
    {
        _logger.LogInformation("Processing step 1 inside scope.");
    }

    [HttpGet("error")]
    public IActionResult GetError()
    {
        try
        {
            throw new InvalidOperationException("Something went wrong processing the order.");
        }
        catch (Exception ex)
        {
            // Serilog.Exceptions will destructure this exception object into JSON
            _logger.LogError(ex, "Failed to process request");
            return StatusCode(500, "Error logged");
        }
    }
}
