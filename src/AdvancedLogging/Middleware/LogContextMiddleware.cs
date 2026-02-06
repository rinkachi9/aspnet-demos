using Serilog.Context;

namespace AdvancedLogging.Middleware;

public class LogContextMiddleware
{
    private readonly RequestDelegate _next;

    public LogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Pushing properties to the LogContext for the duration of the request
        // efficiently.
        using (LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
        {
            await _next(context);
        }
    }
}
