using System.Diagnostics;
using AdvancedTuning.Allocators;

Console.WriteLine($"PID: {Environment.ProcessId}");
Console.WriteLine($"Server GC: {System.Runtime.GCSettings.IsServerGC}");
Console.WriteLine($"Latency Mode: {System.Runtime.GCSettings.LatencyMode}");
Console.WriteLine("Press ENTER to start GC Tuning Benchmarks...");
// Console.ReadLine(); // Commented out for automated testing

var sw = Stopwatch.StartNew();

// 1. Stress Small Object Heap (Gen 0/1)
SohAllocator.Run(1_000_000); // 1 Million objects

// 2. Stress Large Object Heap (LOH)
LohAllocator.Run(500); // 500 x 100KB arrays

sw.Stop();
Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms.");

PrintGcStats();

static void PrintGcStats()
{
    var info = GC.GetGCMemoryInfo();
    
    Console.WriteLine("\n=== GC Stats ===");
    Console.WriteLine($"Heap Size: {info.HeapSizeBytes / 1024 / 1024} MB");
    Console.WriteLine($"Fragmented: {info.FragmentedBytes / 1024 / 1024} MB");
    Console.WriteLine($"Pause Delta: {info.PauseTimePercentage}% time spent in GC");
    
    Console.WriteLine($"Gen 0 Collects: {GC.CollectionCount(0)}");
    Console.WriteLine($"Gen 1 Collects: {GC.CollectionCount(1)}");
    Console.WriteLine($"Gen 2 Collects: {GC.CollectionCount(2)}");
}
