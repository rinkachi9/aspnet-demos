using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ApiDesignPatterns.Controllers;

[ApiController]
[Route("api/greeting")]
public class GreetingController : ControllerBase
{
    private readonly IStringLocalizer<GreetingController> _localizer;

    public GreetingController(IStringLocalizer<GreetingController> localizer)
    {
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult GetGreeting()
    {
        // "Hello" is the key.
        // If "pl-PL" is requested and "Hello" -> "Cześć" exists in resources, it returns "Cześć".
        var message = _localizer["Hello"];
        return Ok(new { Message = message.Value, Culture = Thread.CurrentThread.CurrentCulture.Name });
    }
}
