var builder = WebApplication.CreateBuilder(args);

// 1. Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 2. Map Reverse Proxy
app.MapReverseProxy();

app.Run();
