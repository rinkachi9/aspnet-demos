using Microsoft.AspNetCore.Mvc;

namespace ApiDesignPatterns.Controllers.Pagination;

[ApiController]
[Route("api/pagination/cursor")]
public class CursorPaginationController : ControllerBase
{
    // Mock Data (Id, Value)
    private static readonly List<(int Id, string Value)> _data = Enumerable.Range(1, 1000)
        .Select(i => (i, $"Item {i}"))
        .ToList();

    [HttpGet]
    public IActionResult Get([FromQuery] int? afterId = null, [FromQuery] int size = 10)
    {
        // KEYSET/CURSOR PAGINATION: Where Id > LastId Take Size
        // Pros: Extremely fast (O(1) with index), No "Offset" scan delay.
        // Cons: No random access (Can't jump to page 50). Must be sequential.
        
        var query = _data.AsQueryable();

        if (afterId.HasValue)
        {
            query = query.Where(x => x.Id > afterId.Value);
        }

        var items = query.Take(size).ToList();

        var nextCursor = items.Any() ? items.Last().Id : (int?)null;
        
        var response = new 
        {
            Data = items.Select(x => x.Value),
            Pagination = new 
            {
                Size = size,
                NextCursor = nextCursor,
                NextLink = nextCursor.HasValue ? Url.Action(nameof(Get), new { afterId = nextCursor, size }) : null
            }
        };

        return Ok(response);
    }
}
