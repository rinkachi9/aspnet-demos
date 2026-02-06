using BenchmarkDotNet.Running;
using PerformancePatterns.Benchmarks;

namespace PerformancePatterns;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Use arguments to filter benchmarks (e.g., 'Span' to run only SpanBenchmarks)");
        Console.WriteLine("Available Benchmarks: SpanBenchmarks, SimdBenchmarks, StructVsClassBenchmarks, JsonBenchmarks");
        
        // BenchmarkSwitcher enables running specific benchmarks from CLI args
        var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
