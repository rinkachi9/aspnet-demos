using ApiDesignPatterns.Models;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ApiDesignPatterns.Controllers.v2;

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("2.0")]
public class UsersController : ControllerBase
{
    private readonly IValidator<CreateUserRequest> _validator;

    public UsersController(IValidator<CreateUserRequest> validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // V2 Difference: Returns Enriched Response
        return Ok(new 
        { 
            Id = 1, 
            Data = new { Name = request.FullName, Email = request.Email }, 
            Meta = new { Version = "v2", Timestamp = DateTime.UtcNow } 
        });
    }
}
