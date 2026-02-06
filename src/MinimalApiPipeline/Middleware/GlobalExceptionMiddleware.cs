using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MinimalApiPipeline.Domain;

namespace MinimalApiPipeline.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = ex is UserAlreadyExistsException ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest,
                Title = "Business Rule Violation",
                Detail = ex.Message
            };

            context.Response.StatusCode = problemDetails.Status.Value;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request.",
                Detail = "Internal Server Error"
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
