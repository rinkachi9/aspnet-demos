using System.Net;

namespace SecurityPatterns.Middleware;

public class IpSafelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpSafelistMiddleware> _logger;
    private readonly string[] _safelist;

    public IpSafelistMiddleware(RequestDelegate next, ILogger<IpSafelistMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        // In real app: Load from Config/Azure App Config
        _safelist = configuration.GetSection("Security:IpSafelist").Get<string[]>() ?? Array.Empty<string>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        
        // Handle "X-Forwarded-For" if behind a proxy (Reverse Proxy)
        // Note: UseForwardedHeaders middleware should technically handle this before us, 
        // replacing Connection.RemoteIpAddress with the real IP.
        
        if (remoteIp != null) 
        {
            // Convert IPv4 mapped to IPv6 to pure IPv4 for comparison if needed, or string compare
            var bytes = remoteIp.GetAddressBytes();
            bool isAllowed = false;

            foreach (var safeIp in _safelist)
            {
                // Simple string check (in prod: use IPAddress.Parse / Subnet masking logic)
                if (remoteIp.ToString() == safeIp || safeIp == "*" || IPAddress.Parse(safeIp).Equals(remoteIp))
                {
                    isAllowed = true;
                    break;
                }
            }

            if (!isAllowed)
            {
                _logger.LogWarning("Forbidden Request from IP: {RemoteIp}", remoteIp);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden IP");
                return;
            }
        }

        await _next(context);
    }
}
