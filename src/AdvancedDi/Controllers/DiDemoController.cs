using AdvancedDi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDi.Controllers;

[ApiController]
[Route("di-demo")]
public class DiDemoController(
    IOrderProcessor orderProcessor,
    [FromKeyedServices("email")] INotificationService emailService,
    [FromKeyedServices("sms")] INotificationService smsService,
    IPaymentService paymentService,
    IEnumerable<IRepository> repositories) : ControllerBase
{
    // 1. Decorator Demo
    [HttpPost("order/{id}")]
    public IActionResult ProcessOrder(int id)
    {
        // Should trigger logs from Decorator BEFORE and AFTER
        orderProcessor.ProcessOrder(id);
        return Ok($"Order {id} processed. Check logs for decoration.");
    }

    // 2. Keyed Services Demo
    [HttpPost("notify")]
    public IActionResult Notify([FromQuery] string type, [FromBody] string message)
    {
        var result = type.ToLower() switch
        {
            "email" => emailService.Notify(message),
            "sms" => smsService.Notify(message),
            _ => "Unknown Channel"
        };
        return Ok(result);
    }

    // 3. Conditional DI Demo
    [HttpPost("pay")]
    public IActionResult Pay([FromBody] decimal amount)
    {
        // Implementation depends on Environment (Dev vs Prod)
        return Ok(paymentService.ProcessPayment(amount));
    }

    // 4. Scanning Demo
    [HttpGet("repos")]
    public IActionResult GetRepos()
    {
        // Should return 2 repositories found via Scrutor scanning
        return Ok(repositories.Select(r => r.GetType().Name));
    }
}
