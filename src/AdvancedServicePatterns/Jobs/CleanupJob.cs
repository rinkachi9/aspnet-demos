using Quartz;

namespace AdvancedServicePatterns.Jobs;

public class CleanupJob : IJob
{
    private readonly ILogger<CleanupJob> _logger;
    private readonly string _folderPath;

    public CleanupJob(ILogger<CleanupJob> logger, IHostEnvironment env)
    {
        _logger = logger;
        _folderPath = Path.Combine(env.ContentRootPath, "temp_reports");
    }

    public Task Execute(IJobExecutionContext context)
    {
        if (!Directory.Exists(_folderPath)) return Task.CompletedTask;

        var files = Directory.GetFiles(_folderPath, "*.txt");
        var threshold = DateTime.Now.AddSeconds(-30); // Delete files older than 30s for demo

        foreach (var file in files)
        {
            var creationTime = File.GetCreationTime(file);
            if (creationTime < threshold)
            {
                try
                {
                    File.Delete(file);
                    _logger.LogInformation("CleanupJob deleted old report: {File}", Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete file {File}", file);
                }
            }
        }
        
        return Task.CompletedTask;
    }
}
