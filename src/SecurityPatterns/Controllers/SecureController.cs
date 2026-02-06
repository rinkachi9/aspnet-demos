using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecurityPatterns.Controllers;

[ApiController]
[Route("[controller]")]
public class SecureController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public SecureController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("public")]
    public IActionResult GetPublic()
    {
        return Ok(new { Message = "This endpoint is open to everyone." });
    }

    [HttpGet("user")]
    [Authorize] // Requires VALID JWT
    public IActionResult GetUser()
    {
        return Ok(new { Message = "Hello Authenticated User!", User = User.Identity?.Name });
    }

    [HttpGet("admin")]
    [Authorize(Policy = "MustHaveAdminPermission")] // custom requirement
    public IActionResult GetAdminInfo()
    {
        // Demonstrating Secrets access
        var secretValue = _configuration["MySecretApiKey"];
        return Ok(new { Message = "Admin Section", Secret = secretValue?[..3] + "***" });
    }
    
    [HttpGet("mtls")]
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.Certificate.CertificateAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult GetMtlsInfo()
    {
        return Ok(new { Message = "You authenticated via Mutual TLS (Client Certificate)!" });
    }
}
