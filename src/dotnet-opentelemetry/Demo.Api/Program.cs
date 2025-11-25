using Demo.Api.Extensions;
using Demo.Api.Services;
using Demo.Common.Telemetry;
using OpenTelemetry.Metrics;
using Serilog;

Propagation.ConfigureDefaults();

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

var resourceBuilder = builder.Services.AddApiOpenTelemetry(builder.Configuration);
builder.Logging.AddOpenTelemetryLogging(resourceBuilder, builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddKafkaProducer(builder.Configuration);
builder.Services.AddScoped<IMessagePublisher, KafkaMessagePublisher>();

var app = builder.Build();

app.UseExceptionHandler("/error");
app.UseSerilogRequestLogging();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint();

app.Run();
