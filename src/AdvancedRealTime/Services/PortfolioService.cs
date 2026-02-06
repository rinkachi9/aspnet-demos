using Grpc.Core;

namespace AdvancedRealTime.Services;

public class PortfolioService : Portfolio.PortfolioBase
{
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(ILogger<PortfolioService> logger)
    {
        _logger = logger;
    }

    public override async Task<UploadSummary> BulkUpload(IAsyncStreamReader<PortfolioItem> requestStream, ServerCallContext context)
    {
        int count = 0;
        double totalValue = 0;

        await foreach (var item in requestStream.ReadAllAsync())
        {
            count++;
            totalValue += item.Quantity * item.AvgPrice;
            
            if (count % 100 == 0)
            {
                _logger.LogInformation("Processed {Count} items...", count);
            }
        }

        // Simulate calculation time
        await Task.Delay(500);

        // Add Trailer Metadata (e.g., execution stats)
        var metadata = new Metadata
        {
            { "x-processing-time-ms", "500" },
            { "x-items-processed", count.ToString() }
        };
        await context.WriteResponseHeadersAsync(metadata);

        return new UploadSummary
        {
            TotalItems = count,
            TotalValue = totalValue,
            Status = "COMPLETED"
        };
    }
}
