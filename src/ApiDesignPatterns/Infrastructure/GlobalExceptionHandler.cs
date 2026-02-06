using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace ApiDesignPatterns.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Global Exception Occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationEx)
        {
            problemDetails.Title = "Validation Failed";
            problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
            problemDetails.Type = "https://tools.ietf.org/html/rfc4918#section-11.2";
            problemDetails.Detail = "One or more validation errors occurred.";
            
            problemDetails.Extensions["errors"] = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            
            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        }
        else if (exception is KeyNotFoundException) // Or your custom NotFoundException
        {
            problemDetails.Title = "Resource Not Found";
            problemDetails.Status = StatusCodes.Status404NotFound;
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            problemDetails.Detail = exception.Message;
            
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }
        else
        {
            problemDetails.Title = "An error occurred while processing your request.";
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            problemDetails.Detail = exception.Message; // In production, hide this
            
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
