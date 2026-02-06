using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SignalR.Api.Hubs;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Redis Connection
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var redis = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// 1. SignalR with Redis Backplane
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnection, options => {
        options.Configuration.ChannelPrefix = "AdvancedPlayground";
    });

// 2. JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "AdvancedSignalR",
            ValidAudience = "AdvancedSignalR",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey1234567890_PleaseChangeMe!"))
        };

        // Support SignalR Auth via Query String
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<PresenceHub>("/hubs/presence");

app.MapGet("/", () => "SignalR Advanced Server Running");

app.Run();
