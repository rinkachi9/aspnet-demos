using Demo.Common.Telemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Demo.Api.Extensions;

internal static class OpenTelemetryExtensions
{
    public static ResourceBuilder AddApiOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: Telemetry.ServiceNameApi,
                serviceVersion: Telemetry.ServiceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("service.namespace", "demo"),
                new KeyValuePair<string, object>(
                    "deployment.environment",
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
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
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                    o.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.request_content_length", request.ContentLength);
                        activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress);
                    };
                    o.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.response_content_length", response.ContentLength);
                    };
                })
                .AddHttpClientInstrumentation(o =>
                {
                    o.RecordException = true;
                    o.EnrichWithHttpRequestMessage = (activity, request) =>
                    {
                        activity.SetTag("http.request_content_length", request.Content?.Headers.ContentLength);
                    };
                })
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(otlpEndpoint);
                    o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
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
