using System.Diagnostics;

namespace ApiDesignPatterns.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = GetCorrelationId(context);
        
        // Add to Response Headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = new[] { correlationId };
            return Task.CompletedTask;
        });

        // Add to Logging Scope
        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            // Propagate in diagnostics
            Activity.Current?.SetTag("correlation_id", correlationId);
            
            await _next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
