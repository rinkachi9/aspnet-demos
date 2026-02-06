using AdvancedRealTime.Hubs;
using AdvancedRealTime.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. gRPC
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GlobalExceptionInterceptor>();
    options.EnableDetailedErrors = true;
});

// 2. SIGNALR with REDIS BACKPLANE
// Redis connection string from config or default local
string redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services.AddSignalR()
       .AddStackExchangeRedis(redisConn, options => 
       {
           options.Configuration.ChannelPrefix = "AdvancedRealTime";
       });

var app = builder.Build();

// 3. MAP ENDPOINTS
app.MapGrpcService<MarketDataService>();
app.MapGrpcService<TradingService>();
app.MapGrpcService<PortfolioService>();
app.MapHub<NotificationHub>("/hubs/notifications");

// 4. SSE ENDPOINT (Server-Sent Events)
app.MapGet("/events", async (HttpContext ctx, CancellationToken ct) =>
{
    ctx.Response.Headers.Append("Content-Type", "text/event-stream");
    
    while (!ct.IsCancellationRequested)
    {
        var eventData = new { Time = DateTime.UtcNow, Message = "Update" };
        
        // SSE Format: "data: {json}\n\n"
        await ctx.Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(eventData)}\n\n");
        await ctx.Response.Body.FlushAsync();
        
        await Task.Delay(1000);
    }
});

app.Run();
