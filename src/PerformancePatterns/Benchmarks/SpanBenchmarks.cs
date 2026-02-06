using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace PerformancePatterns.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser] // Tracks allocations
public class SpanBenchmarks
{
    private const string LogLine = "2023-10-27 10:00:00 [INFO] User logged in successfully";

    [Benchmark(Baseline = true)]
    public string ParseDateSubstring()
    {
        // Allocates a new string for the substring
        return LogLine.Substring(0, 10);
    }

    [Benchmark]
    public ReadOnlySpan<char> ParseDateSpan()
    {
        // Zero allocation slicing
        return LogLine.AsSpan().Slice(0, 10);
    }

    [Benchmark]
    public bool CheckLogLevelSubstring()
    {
        // Allocates intermediate string
        var level = LogLine.Substring(21, 4); 
        return level == "INFO";
    }

    [Benchmark]
    public bool CheckLogLevelSpan()
    {
        // Zero allocation comparison
        var slice = LogLine.AsSpan().Slice(21, 4);
        return slice.SequenceEqual("INFO");
    }
}
