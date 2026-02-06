using AdvancedDb.Redis.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Redis
var multiplexer = await ConnectionMultiplexer.ConnectAsync(builder.Configuration["Redis:ConnectionString"]);
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

// Services
builder.Services.AddScoped<RedisLockService>();
builder.Services.AddScoped<GeoService>();
builder.Services.AddScoped<BulkService>();
builder.Services.AddScoped<RateLimiter>();
builder.Services.AddHostedService<StreamConsumerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ENDPOINTS

app.MapPost("/inventory/reserve", async (string resourceId, RedisLockService locker) => 
{
    var lockId = Guid.NewGuid().ToString();
    if (await locker.AcquireLockAsync(resourceId, lockId, TimeSpan.FromSeconds(10)))
    {
        try 
        {
            await Task.Delay(2000); // Simulate work
            return Results.Ok($"Acquired lock {lockId}");
        }
        finally
        {
            await locker.ReleaseLockAsync(resourceId, lockId);
        }
    }
    return Results.Conflict("Locked by another process");
});

app.MapPost("/drivers/location", async (string id, double lat, double lon, GeoService geo) => 
{
    await geo.AddDriverLocationAsync(id, lat, lon);
    return Results.Ok();
});

app.MapGet("/drivers/nearby", async (double lat, double lon, GeoService geo) => 
{
    var drivers = await geo.FindDriversNearbyAsync(lat, lon, 50); // 50km radius
    return Results.Ok(drivers.Select(d => new { d.Member, d.Position }));
});

app.MapPost("/streams/publish", async (string message, IConnectionMultiplexer redis) => 
{
    var db = redis.GetDatabase();
    await db.StreamAddAsync("benchmark-stream", "message", message);
    return Results.Ok("Published");
});

app.MapPost("/benchmark/pipeline", async (int count, BulkService bulk) => 
{
    await bulk.RunPerformanceTestAsync(count);
    return Results.Ok("Check logs for metrics");
});

app.MapGet("/ratelimit/check", async (string user, RateLimiter limiter) => 
{
    if (await limiter.IsAllowedAsync(user, 5, TimeSpan.FromSeconds(10)))
    {
         return Results.Ok("Allowed");
    }
    return Results.StatusCode(429);
});

app.Run();
