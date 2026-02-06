using AdvancedCaching;

var builder = WebApplication.CreateBuilder(args);

// Add services using our Extension method
builder.AddAdvancedCaching();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Critical: UseOutputCache must be before MapControllers
app.UseOutputCache();

app.MapControllers();

app.MapGet("/", () => "Advanced Caching Service Running");

app.Run();
