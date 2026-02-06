using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace AdvancedLogging.Configuration;

public static class SerilogConfiguration
{
    public static void ConfigureLogger(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithExceptionDetails() // Structured Exceptions
            // Global Filter: Exclude noisy HealthChecks from spamming logs
            .Filter.ByExcluding(logEvent => 
                logEvent.Properties.TryGetValue("RequestPath", out var value) &&
                value.ToString().Contains("/health"))
            .WriteTo.Console()
            .WriteTo.Seq(configuration["Seq:ServerUrl"] ?? "http://localhost:5341")
            .CreateLogger();
    }
}
