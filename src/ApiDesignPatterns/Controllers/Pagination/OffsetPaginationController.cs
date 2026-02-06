using ApiDesignPatterns.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiDesignPatterns.Controllers.Pagination;

[ApiController]
[Route("api/pagination/offset")]
public class OffsetPaginationController : ControllerBase
{
    // Mock Data
    private static readonly List<string> _data = Enumerable.Range(1, 1000).Select(i => $"Item {i}").ToList();

    [HttpGet]
    public IActionResult Get([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        // OFFSET PAGINATION: Skip( (Page-1) * Size ).Take( Size )
        // Pros: Simple, Random Access (Go to page 50)
        // Cons: Performance degrades as Skip increases (DB must scan skipped rows). "Stable Sort" required.
        
        var total = _data.Count;
        var items = _data
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        var result = new PagedResult<string>(items, total, page, size);
        
        // HATEOAS
        if (page > 1) 
            result.Links.Add("prev", Url.Action(nameof(Get), new { page = page - 1, size })!);
        
        if (page < result.TotalPages) 
            result.Links.Add("next", Url.Action(nameof(Get), new { page = page + 1, size })!);

        return Ok(result);
    }
}
