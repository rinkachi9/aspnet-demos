using AdvancedAsync.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedAsync.Controllers;

[ApiController]
[Route("async-demo")]
public class AsyncDemoController(
    IBackgroundTaskQueue taskQueue,
    ILogger<AsyncDemoController> logger) : ControllerBase
{
    [HttpPost("fire-and-forget")]
    public async Task<IActionResult> FireAndForget([FromQuery] string name)
    {
        // Enqueue a task that simulates work
        await taskQueue.QueueBackgroundWorkItemAsync(async token =>
        {
            logger.LogInformation("[Task] Starting work for {Name}...", name);
            await Task.Delay(1000, token); // Simulate 1s work
            logger.LogInformation("[Task] Finished work for {Name}.", name);
        });

        logger.LogInformation("Request accepted for {Name}", name);
        return Accepted(new { Status = "Queued", Name = name });
    }

    [HttpPost("bulk/{count}")]
    public async Task<IActionResult> BulkEnqueue(int count)
    {
        logger.LogInformation("Starting Bulk Enqueue of {Count} items...", count);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            int index = i;
            // This await will BLOCK (Backpressure) if the queue is full (Capacity=100)
            // This prevents OOM by slowing down the producer (this API request)
            await taskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                await Task.Delay(100, token); // 100ms work
            });
        }

        sw.Stop();
        logger.LogInformation("Bulk Enqueue finished in {Elapsed}ms", sw.ElapsedMilliseconds);

        return Accepted(new { Status = "Bulk Queued", Count = count, TimeMs = sw.ElapsedMilliseconds });
    }
}
