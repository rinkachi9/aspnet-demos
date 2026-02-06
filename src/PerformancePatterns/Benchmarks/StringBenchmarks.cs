using BenchmarkDotNet.Attributes;
using System.Text;

namespace PerformancePatterns.Benchmarks;

[MemoryDiagnoser]
public class StringBenchmarks
{
    private const int Iterations = 100;

    [Benchmark(Baseline = true)]
    public string String_Concat()
    {
        string result = "";
        for (int i = 0; i < Iterations; i++)
        {
            result += i.ToString();
        }
        return result;
    }

    [Benchmark]
    public string StringBuilder()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Iterations; i++)
        {
            sb.Append(i);
        }
        return sb.ToString();
    }
}
