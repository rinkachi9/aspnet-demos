using Demo.Common.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Serilog;

namespace Demo.Worker.Extensions;

internal static class LoggingExtensions
{
    public static void AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    public static ILoggingBuilder AddOpenTelemetryLogging(
        this ILoggingBuilder loggingBuilder,
        ResourceBuilder resourceBuilder,
        IConfiguration configuration)
    {
        var otlpEndpoint =
            configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint") ??
            Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ??
            "http://otel-collector:4317";

        loggingBuilder.ClearProviders();
        loggingBuilder.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.SetResourceBuilder(resourceBuilder);
            options.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
        });

        return loggingBuilder;
    }
}
