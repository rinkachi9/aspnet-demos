using System.Diagnostics;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ObservabilityPatterns.Diagnostics;
using ObservabilityPatterns.Processors;

var builder = WebApplication.CreateBuilder(args);

// 1. RESOURCE DETECTOR
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(AppDiagnostics.ServiceName, serviceVersion: AppDiagnostics.ServiceVersion)
    .AddTelemetrySdk()
    .AddDetector(new ObservabilityPatterns.Detectors.RegionResourceDetector(builder.Configuration)) // Custom Detector
    .AddEnvironmentVariableDetector();

// 2. CONFIGURE OPENTELEMETRY
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(AppDiagnostics.ActivitySource.Name)
            .AddProcessor(new RedactionProcessor())
            .SetSampler(new TraceIdRatioBasedSampler(0.1)); // HEAD SAMPLING: Keep only 10% of traces
            
        tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri("http://localhost:4317"));
    })
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(AppDiagnostics.Meter.Name)
            // HISTOGRAM VIEWS (Buckets for SLO)
            .AddView("order_processing_seconds", new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 1.0 } 
            })
            // EXEMPLARS are enabled by default in recent OTel SDKs for Prometheus
            .AddPrometheusExporter();
    });

// 3. EXTERNAL SERVICES
builder.Services.AddHttpClient<ObservabilityPatterns.Services.ExternalApiService>()
    .AddStandardResilienceHandler(); // Polly: Retry, CircuitBreaker, Timeout (Requires Microsoft.Extensions.Http.Resilience)

// 4. HEALTH CHECKS
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres") ?? "Host=localhost;Database=db;Username=postgres;Password=postgres")
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379")
    // Kafka Health Check
    .AddKafka(new Confluent.Kafka.ProducerConfig { BootstrapServers = "localhost:9092" }, name: "Kafka")
    // URL Health Check (External API)
    .AddUrlGroup(new Uri("https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current_weather=true"), name: "OpenMeteo API", tags: new[] { "external" })
    .AddCheck("Self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Publish Health Status to Metrics
builder.Services.AddSingleton<Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheckPublisher, ObservabilityPatterns.Health.PrometheusHealthCheckPublisher>();

// Health Check UI (Dashboard)
builder.Services.AddHealthChecksUI(setup =>
{
    setup.AddHealthCheckEndpoint("Local Health", "/health/json");
}).AddInMemoryStorage();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. METRICS ENDPOINT (Prometheus scrapes this)
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// 5. DEMO ENDPOINT (Baggage & Correlation)
app.MapGet("/do-work", async (ILogger<Program> logger) =>
{
    // BAGGAGE: Add context that travels with the request
    Baggage.SetBaggage("tenant.id", "tenant-123");
    Baggage.SetBaggage("user.tier", "gold");

    using var activity = AppDiagnostics.ActivitySource.StartActivity("ManualUnitOfWork");
    activity?.SetTag("user.email", "admin@example.com"); // Should be REDACTED
    activity?.SetTag("order.id", Guid.NewGuid().ToString());

    logger.LogInformation("Doing work for tenant {TenantId}", Baggage.GetBaggage("tenant.id"));

    // Simulate work
    await Task.Delay(100);
    
    // Increment Metric
    AppDiagnostics.OrdersPlaced.Add(1, new KeyValuePair<string, object?>("tier", "gold"));

    return Results.Ok(new { Status = "Work Done", TraceId = activity?.TraceId.ToString() });
});

app.MapGet("/weather", async (ObservabilityPatterns.Services.ExternalApiService weatherService) =>
{
    var data = await weatherService.GetCurrentWeatherAsync();
    return Results.Ok(data);
});

// 6. HEALTH ENDPOINTS
app.MapHealthChecks("/health/json", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(config => config.UIPath = "/health-dashboard");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
