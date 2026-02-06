using AdvancedAsync.Infrastructure;
using AdvancedAsync.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Core Services
// Singleton is correct for the Queue (shared state)
builder.Services.AddSingleton<QueueMetrics>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

// 2. Worker
builder.Services.AddHostedService<QueueHostedService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
