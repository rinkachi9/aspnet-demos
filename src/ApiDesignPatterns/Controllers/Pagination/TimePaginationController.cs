using Microsoft.AspNetCore.Mvc;

namespace ApiDesignPatterns.Controllers.Pagination;

[ApiController]
[Route("api/pagination/time")]
public class TimePaginationController : ControllerBase
{
    // Mock Data (Timestamp, Value) - Sorted Descending (Newest first)
    private static readonly List<(DateTime Created, string Value)> _data = Enumerable.Range(1, 1000)
        .Select(i => (DateTime.UtcNow.AddMinutes(-i), $"Event {i}")) // Event 1 is newest (-1 min)
        .OrderByDescending(x => x.Created)
        .ToList();

    [HttpGet]
    public IActionResult Get([FromQuery] long? beforeUnix = null, [FromQuery] int size = 10)
    {
        // TIME CURSOR: Where CreatedAt < LastSeenTimestamp Take Size
        // Common in feeds (Twitter, FB, Logs).
        
        var query = _data.AsQueryable();

        if (beforeUnix.HasValue)
        {
            var date = DateTimeOffset.FromUnixTimeSeconds(beforeUnix.Value).UtcDateTime;
            query = query.Where(x => x.Created < date);
        }

        var items = query.Take(size).ToList();
        
        long? nextCursor = items.Any() 
            ? ((DateTimeOffset)items.Last().Created).ToUnixTimeSeconds() 
            : null;

        var response = new 
        {
            Data = items.Select(x => new { x.Created, x.Value }),
            Pagination = new 
            {
                Size = size,
                NextCursor = nextCursor, // Unix Timestamp
                NextLink = nextCursor.HasValue ? Url.Action(nameof(Get), new { beforeUnix = nextCursor, size }) : null
            }
        };

        return Ok(response);
    }
}
