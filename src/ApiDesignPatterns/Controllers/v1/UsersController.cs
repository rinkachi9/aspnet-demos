using ApiDesignPatterns.Models;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ApiDesignPatterns.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1.0")]
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
        // Manual validation trigger to demonstrate Control Flow -> Exception -> ProblemDetails
        // Alternatively, use [FluentValidationAutoValidation] in Program.cs
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return Ok(new { Id = 1, Name = request.FullName, Version = "v1" });
    }
}
