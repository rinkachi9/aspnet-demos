using AdvancedTimezones.Models;
using AdvancedTimezones.Services;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace AdvancedTimezones.Controllers;

[ApiController]
[Route("[controller]")]
public class TimeController : ControllerBase
{
    private readonly MeetingSchedulingService _service;
    private readonly IClock _clock;

    public TimeController(MeetingSchedulingService service, IClock clock)
    {
        _service = service;
        _clock = clock;
    }

    [HttpGet("now")]
    public IActionResult GetNow()
    {
        // Always return Instant for machine-to-machine comms
        return Ok(new { CurrentUtc = _clock.GetCurrentInstant() });
    }

    [HttpPost("schedule")]
    public IActionResult Schedule([FromBody] MeetingRequest request)
    {
        try
        {
            var result = _service.ScheduleMeeting(request);
            return Ok(result);
        }
        catch (DateTimeZoneNotFoundException)
        {
            return BadRequest("Invalid Time Zone ID");
        }
    }
}
