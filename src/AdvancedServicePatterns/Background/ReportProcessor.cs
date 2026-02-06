using AdvancedServicePatterns.Models;
using AdvancedServicePatterns.Services;

namespace AdvancedServicePatterns.Background;

public class ReportProcessor : BackgroundService
{
    private readonly ITaskQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportProcessor> _logger;
    private readonly string _outputFolder;

    public ReportProcessor(ITaskQueue queue, IServiceScopeFactory scopeFactory, ILogger<ReportProcessor> logger, IHostEnvironment env)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _outputFolder = Path.Combine(env.ContentRootPath, "temp_reports");
        
        if (!Directory.Exists(_outputFolder))
        {
            Directory.CreateDirectory(_outputFolder);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReportProcessor started. Watching queue...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try 
            {
                var request = await _queue.DequeueAsync(stoppingToken);

                // 1. Generate Physical File (Simulate I/O)
                var fileName = $"Report_{request.Id}_{DateTime.Now:yyyyMMddHHmmss}.txt";
                var filePath = Path.Combine(_outputFolder, fileName);
                
                await File.WriteAllTextAsync(filePath, $"Report Content for {request.ReportName}\nGenerated at: {DateTime.Now}", stoppingToken);
                _logger.LogInformation("Generated Report File: {FilePath}", filePath);

                // 2. Consume Scoped Services & Strategy
                using var scope = _scopeFactory.CreateScope();
                
                // Select Strategy based on Request Type via Keyed Services
                var notifyKey = request.NotifyType.ToLower() == "sms" ? "sms" : "email";
                var notifier = scope.ServiceProvider.GetRequiredKeyedService<INotificationService>(notifyKey);

                await notifier.NotifyAsync($"Your report '{request.ReportName}' is ready.");
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing report");
            }
        }
    }
}
