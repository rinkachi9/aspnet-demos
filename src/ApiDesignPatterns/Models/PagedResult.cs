using Microsoft.AspNetCore.Mvc;

namespace ApiDesignPatterns.Models;

public class PagedRequest
{
    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    [FromQuery(Name = "size")]
    public int PageSize { get; set; } = 10;
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    // HATEOAS Links
    public Dictionary<string, string> Links { get; set; } = new();

    public PagedResult(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
