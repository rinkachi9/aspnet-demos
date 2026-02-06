namespace SecurityPatterns.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. HSTS (Strict-Transport-Security) - Enforced by App, but manual fallback here
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        // 2. X-Content-Type-Options - Prevent MIME sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // 3. X-Frame-Options - Prevent Clickjacking (Deny frames)
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // 4. CSP (Content-Security-Policy) - The big one
        // Default: Allow only self, no inline scripts, no external objects
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; " + 
            "img-src 'self' data:; " + 
            "script-src 'self'; " + 
            "frame-ancestors 'none';");

        // 5. Referrer-Policy - Privacy
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
