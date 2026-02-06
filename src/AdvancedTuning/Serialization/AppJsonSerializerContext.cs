using System.Text.Json.Serialization;

namespace AdvancedTuning.Serialization;

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// SOURCE GENERATOR CONTEXT
// Generates C# serialization code at compile time.
// Faster startup, lower memory (no reflection metadata), AOT compatible.
[JsonSerializable(typeof(List<WeatherForecast>))]
[JsonSerializable(typeof(WeatherForecast))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
