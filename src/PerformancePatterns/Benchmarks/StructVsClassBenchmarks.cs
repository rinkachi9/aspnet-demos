using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace PerformancePatterns.Benchmarks;

public class PointClass
{
    public int X { get; set; }
    public int Y { get; set; }
}

public struct PointStruct
{
    public int X { get; set; }
    public int Y { get; set; }
}

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class StructVsClassBenchmarks
{
    private PointClass[] _classes;
    private PointStruct[] _structs;

    [Params(100_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _classes = new PointClass[Count];
        _structs = new PointStruct[Count];

        for (int i = 0; i < Count; i++)
        {
            _classes[i] = new PointClass { X = i, Y = i };
            _structs[i] = new PointStruct { X = i, Y = i };
        }
    }

    [Benchmark(Baseline = true)]
    public int SumClasses()
    {
        int sum = 0;
        // Classes are references, scattered in heap. Processing them involves pointer chasing and cache misses.
        foreach (var item in _classes)
        {
            sum += item.X + item.Y;
        }
        return sum;
    }

    [Benchmark]
    public int SumStructs()
    {
        int sum = 0;
        // Structs are packed contiguously in memory array. Better spatial locality = fewer cache misses.
        foreach (var item in _structs)
        {
            sum += item.X + item.Y;
        }
        return sum;
    }
}
