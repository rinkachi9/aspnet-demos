using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace PerformancePatterns.Benchmarks;

// DTO
public class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}

// Source Generator Context
[JsonSerializable(typeof(WeatherForecast))]
[JsonSerializable(typeof(List<WeatherForecast>))]
public partial class WeatherContext : JsonSerializerContext
{
}

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class JsonBenchmarks
{
    private List<WeatherForecast> _data;
    private JsonSerializerOptions _checkOptions;

    [Params(100, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(1, Count).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = "Freezing"
        }).ToList();
        
        _checkOptions = new JsonSerializerOptions(); // Default reflection options
    }

    [Benchmark(Baseline = true)]
    public string SerializeReflection()
    {
        // Uses Reflection at runtime to inspect properties
        return JsonSerializer.Serialize(_data, _checkOptions);
    }

    [Benchmark]
    public string SerializeSourceGen()
    {
        // Uses compile-time generated code (optimized metadata)
        return JsonSerializer.Serialize(_data, WeatherContext.Default.ListWeatherForecast);
    }
}
