using BackgroundJobsQuartz.Attributes;
using BackgroundJobsQuartz.Options;
using Quartz;

namespace BackgroundJobsQuartz.Jobs;

// Run every 10 seconds
[QuartzJob("0/10 * * * * ?", Identity = "ReportGenerator", Group = "Reports")]
public class ReportGeneratorJob : IJob
{
    private readonly ILogger<ReportGeneratorJob> _logger;
    private readonly ReportJobOptions _options;

    public ReportGeneratorJob(ILogger<ReportGeneratorJob> logger, Microsoft.Extensions.Options.IOptions<ReportJobOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Generating '{ReportName}' at {Time} (Retry Limit: {RetryCount})...", 
            _options.ReportName, DateTime.UtcNow, _options.RetryCount);
            
        return Task.CompletedTask;
    }
}

// Run every minute
[QuartzJob("0 * * * * ?", Identity = "DatabaseCleanup")]
public class DatabaseCleanupJob : IJob
{
    private readonly ILogger<DatabaseCleanupJob> _logger;

    public DatabaseCleanupJob(ILogger<DatabaseCleanupJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Cleaning Database at {Time}...", DateTime.UtcNow);
        return Task.CompletedTask;
    }
}
