using System.IO.Compression;
using ApiDesignPatterns.Middleware;
using ApiDesignPatterns.Models;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Distributed;
using ProtoBuf.Meta;

var builder = WebApplication.CreateBuilder(args);

// 1. Services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddExceptionHandler<ApiDesignPatterns.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); // Default fallback
builder.Services.AddValidatorsFromAssemblyContaining<ApiDesignPatterns.Models.CreateUserRequestValidator>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 1.1 Content Negotiation & Controllers
// Using Controllers along with Minimal API to demo rich formatting support easily
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true; // Essential!
    options.ReturnHttpNotAcceptable = true; // 406 if format not supported
})
.AddXmlSerializerFormatters() // Adds XML Support
.AddProtobufFormatters();     // Adds Protobuf Support

// 1.2 Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddRequestDecompression();

// 1.3 Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var supportedCultures = new[] { "en-US", "pl-PL" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// 2. Middleware Pipeline

// 2.1 Global Error Handling (ProblemDetails)
app.UseExceptionHandler();
// app.UseMiddleware<GlobalErrorHandlerMiddleware>(); // Replacing old custom middleware

// 2.2 Correlation ID
app.UseMiddleware<CorrelationIdMiddleware>();

// 2.3 Compression
app.UseResponseCompression();
app.UseRequestDecompression();

// 2.4 Conditional Middleware (UseWhen)
// Only log Public API requests, skip Health checks or specific paths if needed
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseMiddleware<RequestResponseLoggingMiddleware>();
});

// 2.5 Pipeline Branching (MapWhen)
// Isolated pipeline for admin routes (e.g., might require different auth middleware stack)
app.MapWhen(context => context.Request.Path.StartsWithSegments("/admin"), adminBuilder =>
{
    // adminBuilder.UseAuthentication(); // e.g.
    adminBuilder.Run(async context =>
    {
        await context.Response.WriteAsync("Admin Area - Isolated Pipeline Branch");
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var desc in descriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{desc.GroupName}/swagger.json",
                desc.GroupName.ToUpperInvariant());
        }
    });
}

// 2.6 Idempotency
app.UseMiddleware<IdempotencyMiddleware>();

app.MapControllers(); // Enable Controller Routes

// 3. API Version Set (Minimal API)
var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

app.MapGet("/api/v{version:apiVersion}/minimal-proto", () =>
{
    return new ProductDto
    {
        Id = 101,
        Name = "Minimal Product (Only JSON by default)",
        Price = 12.50m,
        Category = "Demo"
    };
})
.WithApiVersionSet(apiVersionSet)
.MapToApiVersion(1, 0)
.WithOpenApi();

app.Run();
