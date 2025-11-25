using Demo.Common.Telemetry;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Controllers;

[ApiController]
public class StatusController : ControllerBase
{
    [HttpGet("/")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Root() => Ok(new { ok = true, service = Telemetry.ServiceNameApi });

    [HttpGet("/health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health() => Ok(new { status = "ok", service = Telemetry.ServiceNameApi });

    [HttpGet("/error")]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        return Problem(detail: feature?.Error.Message);
    }
}
