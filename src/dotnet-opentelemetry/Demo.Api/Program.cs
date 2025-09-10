using System.Diagnostics;
using Confluent.Kafka;
using Demo.Common.Messaging;
using Demo.Common.Messaging.Contracts;
using Demo.Common.Telemetry;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;
using Serilog;
using Baggage = OpenTelemetry.Baggage;

var builder = WebApplication.CreateBuilder(args);

// ----- Serilog (console) -----
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// ----- OpenTelemetry -----
builder.Services.AddOpenTelemetry()
    .ConfigureResource(rb => rb.AddService(serviceName: Telemetry.ServiceNameApi))
    .WithTracing(t => t
        .AddSource(Telemetry.ActivitySource.Name)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://otel-collector:4317");
            o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter()); // /metrics

builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeFormattedMessage = true;
    o.IncludeScopes = true;
    o.ParseStateValues = true;
    o.AddOtlpExporter();
});

// Kafka producer
var kafkaBootstrap = builder.Configuration["KAFKA__BROKERS"] ?? "redpanda:9092";
var producerConfig = new ProducerConfig
{
    BootstrapServers = kafkaBootstrap,
    Acks = Acks.All,
    EnableIdempotence = true,
};
builder.Services.AddSingleton(new ProducerBuilder<string, string>(producerConfig).Build());

var app = builder.Build();

// Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

app.MapGet("/", () => Results.Ok(new { ok = true, service = Telemetry.ServiceNameApi }));

app.MapPost("/publish", async ([FromBody] string payload, IProducer<string,string> producer) =>
{
    using var activity = Telemetry.ActivitySource.StartActivity("PublishMessage", ActivityKind.Producer);
    activity?.SetTag("messaging.system", "kafka");
    activity?.SetTag("messaging.destination", Topics.Sample);

    var msg = new SampleMessage(Guid.NewGuid(), DateTimeOffset.UtcNow, payload);
    var headers = new Headers();

    // Inject W3C context into Kafka headers
    var propagator = System.Diagnostics.Activity.Current?.IdFormat == ActivityIdFormat.W3C
        ? System.Diagnostics.Propagators.DistributedContextPropagator.Current
        : System.Diagnostics.Propagators.DistributedContextPropagator.CreateDefault();

    propagator.Inject(new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current), headers, (h, k, v) =>
    {
        h.Add(k, System.Text.Encoding.UTF8.GetBytes(v));
    });

    var value = System.Text.Json.JsonSerializer.Serialize(msg);
    await producer.ProduceAsync(Topics.Sample, new Message<string,string>
    {
        Key = msg.Id.ToString(),
        Value = value,
        Headers = headers
    });

    Log.Information("Published message {Id}", msg.Id);
    return Results.Accepted($"/status/{msg.Id}", new { id = msg.Id });
});

app.Run();
