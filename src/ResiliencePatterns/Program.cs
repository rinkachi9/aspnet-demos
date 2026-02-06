using System.Threading.RateLimiting;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using ResiliencePatterns.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Redis Connection
// "localhost:6379" works because we run app locally and Redis in Docker
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

// 2. Output Caching (Distributed via Redis)
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = redisConnection;
});
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));
    options.AddPolicy("NoCache", builder => builder.NoCache());
    options.AddPolicy("ByQuery", builder => builder.SetVaryByQuery("city").Expire(TimeSpan.FromSeconds(30)));
});

// 3. Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global limiter (Bucket) - simple example
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
            
    // Specific Policy
    options.AddPolicy("Strict", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 3,
                QueueLimit = 0,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                TokensPerPeriod = 1,
                AutoReplenishment = true
            }));
});

// 4. Resilience (Polly)
builder.Services.AddHttpClient<IExternalWeatherService, ExternalWeatherService>()
    .AddStandardResilienceHandler(options => 
    {
        // 1. Retry Strategy
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromMilliseconds(500); // Wait 500ms between retries

        // 2. Attempt Timeout Strategy (Timeout per individual request)
        // Must be less than TotalRequestTimeout and sampling duration should consider valid minimums
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2); 

        // 3. Circuit Breaker Strategy
        // SamplingDuration must be >= 2 * AttemptTimeout
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5); 
        options.CircuitBreaker.FailureRatio = 0.5;

        // 4. Total Request Timeout Strategy (Timeout for all retries combined)
        // Must be > AttemptTimeout
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10); 
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();
app.UseOutputCache();

// Endpoints

app.MapGet("/weather", async (string city, IExternalWeatherService service) =>
{
    return await service.GetCurrentWeatherAsync(city);
})
.CacheOutput("ByQuery") // Caches per unique 'city', expires in 30s
.WithOpenApi();

app.MapGet("/limited", () =>
{
    return Results.Ok("You passed the rate limiter!");
})
.RequireRateLimiting("Strict")
.WithOpenApi();

app.MapGet("/evict", async (IOutputCacheStore store, CancellationToken ct) =>
{
    await store.EvictByTagAsync("weather", ct);
    return Results.Ok("Cache evicted");
});

app.Run();
