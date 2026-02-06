using AdvancedSecurity.Authorization;
using AdvancedSecurity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedSecurity.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    // Simulate DB
    private readonly List<Document> _documents = new()
    {
        new() { Id = 1, Title = "Alice's Diary", Author = "alice" },
        new() { Id = 2, Title = "Bob's Plans", Author = "bob" }
    };

    public DocumentsController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        var doc = _documents.FirstOrDefault(d => d.Id == id);
        if (doc == null) return NotFound();

        // Resource-based AuthZ: Check if User can "Read" (or Edit) this specific document
        // We reuse SameAuthorRequirement for demo purposes as "EditPolicy"
        var result = await _authorizationService.AuthorizeAsync(User, doc, "EditPolicy");

        if (!result.Succeeded)
        {
            return Forbid();
        }

        return Ok(doc);
    }
}
