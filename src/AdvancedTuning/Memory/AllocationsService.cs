using System.Buffers;
using System.Text;

namespace AdvancedTuning.Memory;

public class AllocationsService
{
    // ARRAY POOL: Avoids allocating new arrays on LOH for large buffers.
    // Reuses arrays from a shared pool.
    public void ProcessLargeData(int size)
    {
        var pool = ArrayPool<byte>.Shared;
        byte[] buffer = pool.Rent(size); // RENT (O(1))

        try
        {
            // Simulate work
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(i % 255);
            }
        }
        finally
        {
            pool.Return(buffer); // RETURN (Crucial!)
        }
    }

    // SPAN: Lightweight, zero-allocation "view" over memory.
    // Parses string without creating substrings (new String = allocation).
    public (string Scheme, string Path) ParseUrlZeroAlloc(string url)
    {
        ReadOnlySpan<char> span = url.AsSpan();
        
        int schemeIndex = span.IndexOf("://".AsSpan());
        if (schemeIndex == -1) return (string.Empty, string.Empty);

        // Slice without allocation
        ReadOnlySpan<char> schemeSpan = span.Slice(0, schemeIndex);
        
        int pathIndex = span.IndexOf('/', schemeIndex + 3);
        ReadOnlySpan<char> pathSpan = pathIndex != -1 ? span.Slice(pathIndex) : ReadOnlySpan<char>.Empty;

        return (schemeSpan.ToString(), pathSpan.ToString()); // Only allocate final strings
    }
}
