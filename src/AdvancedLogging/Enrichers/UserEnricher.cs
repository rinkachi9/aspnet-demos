using Serilog.Core;
using Serilog.Events;

namespace AdvancedLogging.Enrichers;

public class UserEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // Extract User ID (if authenticated) or trace identifier
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.Identity.Name ?? "auth_user";
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
        }

        // Always useful to have TraceIdentifier if not using built-in request logging
        // (Though UseSerilogRequestLogging adds RequestId usually)
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceIdentifier", httpContext.TraceIdentifier));
    }
}
