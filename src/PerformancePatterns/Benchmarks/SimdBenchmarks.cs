using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace PerformancePatterns.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
public class SimdBenchmarks
{
    private int[] _data;

    [Params(1000, 100_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, Count).ToArray();
    }

    [Benchmark(Baseline = true)]
    public int SumLoop()
    {
        var sum = 0;
        foreach (var item in _data)
        {
            sum += item;
        }
        return sum;
    }

    [Benchmark]
    public int SumVector()
    {
        var vectorSize = Vector<int>.Count;
        var accVector = Vector<int>.Zero;
        var i = 0;
        var span = _data.AsSpan();

        // Process in chunks of Vector<int>.Count (e.g., 4 or 8 integers at once)
        for (; i <= span.Length - vectorSize; i += vectorSize)
        {
            var v = new Vector<int>(span.Slice(i));
            accVector += v;
        }

        // Sum the elements of the accumulator vector
        var result = Vector.Dot(accVector, Vector<int>.One);

        // Process remaining elements
        for (; i < span.Length; i++)
        {
            result += span[i];
        }

        return result;
    }
}
