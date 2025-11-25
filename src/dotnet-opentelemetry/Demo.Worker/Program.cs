using Demo.Common.Telemetry;
using Demo.Worker.Extensions;
using Demo.Worker.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Propagation.ConfigureDefaults();

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

var resourceBuilder = builder.Services.AddWorkerOpenTelemetry(builder.Configuration);
builder.Logging.AddOpenTelemetryLogging(resourceBuilder, builder.Configuration);

builder.Services.AddHealthChecks();
builder.Services.AddKafkaConsumer(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint();

await app.RunAsync();
