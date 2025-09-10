using System.Diagnostics;
using System.Text;
using Demo.Common.Messaging;
using Demo.Common.Telemetry;
using Demo.Worker.Handlers;
using Demo.Worker.Middlewares;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);

// Serilog (console)
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.UseSerilog();

builder.ConfigureServices((ctx, services) =>
{
    // OpenTelemetry
    services.AddOpenTelemetry()
        .ConfigureResource(rb => rb.AddService(Telemetry.ServiceNameWorker))
        .WithTracing(t => t
            .AddSource(Telemetry.ActivitySource.Name)
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://otel-collector:4317");
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            }))
        .WithMetrics(m => m
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddPrometheusExporter()); // we'll host /metrics via Kestrel below

    services.AddLogging(lb =>
    {
        lb.ClearProviders();
        lb.AddOpenTelemetry(o =>
        {
            o.IncludeFormattedMessage = true;
            o.IncludeScopes = true;
            o.ParseStateValues = true;
            o.AddOtlpExporter();
        });
    });

    var brokers = ctx.Configuration["KAFKA__BROKERS"] ?? "redpanda:9092";

    services.AddKafka(k => k
        .UseConsoleLog()
        .AddCluster(c => c
            .WithBrokers(new[] { brokers })
            .CreateTopicIfNotExists(Topics.Sample, 1, 1)
            .AddConsumer(consumer => consumer
                .Topic(Topics.Sample)
                .WithGroupId("sample-consumer-group")
                .WithBufferSize(100)
                .WithWorkersCount(1)
                .AddMiddlewares(mw => mw
                    .AddSerializer<JsonCoreSerializer>()
                    .Add<TracingMiddleware>() // custom tracing extractor
                    .Add<SampleMessageHandler>()))));

    // Hosted minimal web server for Prometheus /metrics
    services.AddSingleton(provider =>
    {
        var webBuilder = WebApplication.CreateBuilder();
        webBuilder.Services.AddOpenTelemetry(); // use existing meters
        var app = webBuilder.Build();
        app.MapPrometheusScrapingEndpoint();
        return app;
    });

    services.AddHostedService<WebServerHostedService>();
});

var host = builder.Build();
await host.RunAsync();

// --- Custom types ---
public sealed class WebServerHostedService : IHostedService
{
    private readonly WebApplication _app;
    private Task? _runTask;

    public WebServerHostedService(WebApplication app) => _app = app;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _runTask = _app.RunAsync(cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_runTask is not null)
            await _app.StopAsync(cancellationToken);
    }
}


