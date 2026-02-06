using GrpcService.Api.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTP/2 (Unencrypted)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5130, o => o.Protocols = HttpProtocols.Http2);
});

// 1. Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// 2. Configure the HTTP request pipeline.
app.MapGrpcService<WeatherService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
