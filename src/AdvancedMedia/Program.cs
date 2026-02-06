using AdvancedMedia.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Blob Service
builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<SasTokenService>();
builder.Services.AddSingleton<WatermarkService>();
builder.Services.AddHostedService<MediaBackgroundProcessor>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
