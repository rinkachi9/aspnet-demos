using AdvancedTimezones.Services;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure NodaTime Serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // THIS IS CRITICAL: Maps Instant -> String, ZonedDateTime -> String correctly
        options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Register NodaTime Services
builder.Services.AddSingleton<IClock>(SystemClock.Instance);
builder.Services.AddSingleton<IDateTimeZoneProvider>(DateTimeZoneProviders.Tzdb);
builder.Services.AddScoped<MeetingSchedulingService>();
builder.Services.AddScoped<FlightService>();
builder.Services.AddScoped<RecurrenceService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
