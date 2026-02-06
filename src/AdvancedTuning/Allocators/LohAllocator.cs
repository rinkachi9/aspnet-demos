namespace AdvancedTuning.Allocators;

public class LohAllocator
{
    public static void Run(int iterations)
    {
        Console.WriteLine($"[LOH] Allocating {iterations} large arrays (>85KB)...");
        
        var graveyard = new List<byte[]>();

        for (int i = 0; i < iterations; i++)
        {
            // Allocate 100KB array (LOH threshold is 85000 bytes)
            var buffer = new byte[1024 * 100]; 
            
            // Touch memory to ensure OS pages are committed
            buffer[0] = 0xFF;
            buffer[^1] = 0xFF;

            if (i % 2 == 0)
            {
                graveyard.Add(buffer); // Retain
            }
            // Else: Let it go out of scope (become garbage immediately)
        }

        Console.WriteLine($"[LOH] Finished {iterations} allocs. Retained: {graveyard.Count}");
    }
}
