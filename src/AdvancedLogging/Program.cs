using AdvancedLogging.Configuration;
using AdvancedLogging.Enrichers;
using AdvancedLogging.Middleware;
using Serilog;

// 1. BOOTSTRAP LOGGER (Two-Stage Initialization)
// Catch startup errors that happen before Host is built.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application...");

    var builder = WebApplication.CreateBuilder(args);

    // 2. CONFIGURE SERILOG
    // Switches from Bootstrap Logger to Full Logger defined in SerilogConfiguration
    builder.Host.UseSerilog((context, services, configuration) => 
        SerilogConfiguration.ConfigureLogger(context.Configuration));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHttpContextAccessor();
    
    // Register Custom Enricher
    builder.Services.AddTransient<UserEnricher>();

    var app = builder.Build();

    // 3. MIDDLEWARE PIPELINE
    // "UseSerilogRequestLogging" must be AFTER StaticFiles but BEFORE MVC/Endpoints
    // It captures the entire pipeline duration.
    app.UseSerilogRequestLogging(options =>
    {
        // Customize the message template for cleaner logs
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        
        // Emit debug information for every request
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            // Verify if UserEnricher is working (it runs as ILogEventEnricher, but we can also push here)
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Custom Middleware to push LogContext properties
    app.UseMiddleware<LogContextMiddleware>();

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
