namespace AdvancedTuning.Allocators;

public class SohAllocator
{
    public static void Run(int objectCount)
    {
        Console.WriteLine($"[SOH] Allocating {objectCount} small objects...");
        var list = new List<string>(objectCount);
        
        for (int i = 0; i < objectCount; i++)
        {
            // Allocate small strings (approx 20-50 bytes)
            // They will likely die in Gen0 (short-lived) or survive to Gen1/2 if held in list.
            list.Add($"Item-{i}-{Guid.NewGuid()}"); 
        }

        Console.WriteLine($"[SOH] Allocated {list.Count} items. Retaining some, clearing others.");
        
        // Clear half to induce garbage
        list.RemoveRange(0, objectCount / 2);
    }
}
