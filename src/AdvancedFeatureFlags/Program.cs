using AdvancedFeatureFlags.Endpoints;
using AdvancedFeatureFlags.Services;
using AdvancedFeatureFlags.Workers;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

// 1. Basic Feature Flags
builder.Services.AddFeatureManagement();

// 2. Unleash (Singleton needed for Client)
builder.Services.AddSingleton<IUnleashService, UnleashService>();

// 3. Worker
builder.Services.AddHostedService<DynamicWorker>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

FeaturesEndpoint.Map(app);

app.Run();
