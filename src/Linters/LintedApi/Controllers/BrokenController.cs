using Microsoft.AspNetCore.Mvc;

namespace LintedApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BrokenController : ControllerBase
{
    // This violates API001 because it has uppercase letters
    [HttpGet("GetUsers")] 
    public IActionResult GetUsers()
    {
        return Ok("Use the code fix to change 'GetUsers' to 'get-users'");
    }
    
    // This is valid
    [HttpGet("get-valid")]
    public IActionResult GetValid()
    {
        return Ok();
    }
}
