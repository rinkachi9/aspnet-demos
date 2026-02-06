using System.Text;

namespace ApiDesignPatterns.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Demonstrate .Use() behavior: Action before Next
        var req = context.Request;
        _logger.LogInformation("Processing {Method} {Path}{Query}", req.Method, req.Path, req.QueryString);

        // Capture Response (if tracking body/size needed) or just timing
        // For production, avoid reading body in memory unless strictly necessary.
        
        await _next(context);

        // Action after Next
        _logger.LogInformation("Completed {StatusCode}", context.Response.StatusCode);
    }
}
