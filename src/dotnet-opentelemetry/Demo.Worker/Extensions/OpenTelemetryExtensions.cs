using Demo.Common.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Demo.Worker.Extensions;

internal static class OpenTelemetryExtensions
{
    public static ResourceBuilder AddWorkerOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: Telemetry.ServiceNameWorker,
                serviceVersion: Telemetry.ServiceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("service.namespace", "demo"),
                new KeyValuePair<string, object>(
                    "deployment.environment",
                    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                    "Production")
            });

        var otlpEndpoint =
            configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint") ??
            Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ??
            "http://otel-collector:4317";

        services.AddOpenTelemetry()
            .WithTracing(tracer => tracer
                .SetResourceBuilder(resourceBuilder)
                .SetSampler(new ParentBasedSampler(new AlwaysOnSampler()))
                .AddSource(Telemetry.ActivitySource.Name)
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation(o => o.RecordException = true)
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(otlpEndpoint);
                    o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter(Metrics.Meter.Name)
                .AddPrometheusExporter()
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(otlpEndpoint);
                    o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                }));

        return resourceBuilder;
    }
}
