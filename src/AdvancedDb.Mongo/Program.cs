using AdvancedDb.Mongo.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Mongo Registration
var mongoSettings = MongoClientSettings.FromConnectionString(builder.Configuration["Mongo:ConnectionString"]);
var client = new MongoClient(mongoSettings);
var database = client.GetDatabase(builder.Configuration["Mongo:DatabaseName"]);

builder.Services.AddSingleton<IMongoClient>(client);
builder.Services.AddSingleton<IMongoDatabase>(database);
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<SalesAnalyticsService>();
builder.Services.AddHostedService<OrderWatcher>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ENDPOINTS

app.MapPost("/init/indexes", async (ProductService service) => 
{
    await service.InitializeIndexesAsync();
    return Results.Ok("Indexes Created");
});

app.MapPost("/init/bulk/{count}", async (int count, ProductService service) => 
{
    await service.BulkInsertAsync(count);
    return Results.Ok($"Inserted {count}");
});

app.MapGet("/products", async (ProductService service) => await service.GetAllAsync());

app.MapPost("/purchase", async (string productId, int quantity, ProductService service) => 
{
    try 
    {
        await service.PurchaseAsync(productId, quantity);
        return Results.Ok("Purchased!");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPut("/products/{id}", async (string id, string name, int version, ProductService service) => 
{
    try
    {
        await service.UpdateDetailsAsync(id, name, version);
        return Results.Ok("Updated!");
    }
    catch (Exception ex)
    {
        return Results.Conflict(ex.Message);
    }
});

app.MapGet("/analytics/report", async (SalesAnalyticsService service) => 
{
    var report = await service.GetComprehensiveReportAsync();
    // Return raw BSON as JSON string for demo
    return Results.Content(report.ToJson(), "application/json");
});

app.Run();
