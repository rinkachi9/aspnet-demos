using System.Diagnostics.Metrics;
using Quartz;

namespace BackgroundJobsQuartz.Listeners;

public class GlobalJobListener : IJobListener
{
    private readonly ILogger<GlobalJobListener> _logger;
    private readonly Counter<long> _jobExecutionCounter;
    private readonly Histogram<double> _jobDurationHistogram;

    public GlobalJobListener(ILogger<GlobalJobListener> logger)
    {
        _logger = logger;
        
        // Setup Metrics
        var meter = new Meter("BackgroundJobsQuartz", "1.0.0");
        _jobExecutionCounter = meter.CreateCounter<long>("jobs_executed_total", description: "Total number of job executions");
        _jobDurationHistogram = meter.CreateHistogram<double>("job_duration_seconds", unit: "s", description: "Duration of job execution");
    }

    public string Name => "GlobalJobListener";

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation(">> Starting Job: {JobKey}", context.JobDetail.Key);
        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        _logger.LogWarning(">> Job Vetoed: {JobKey}", context.JobDetail.Key);
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken)
    {
        var duration = context.JobRunTime.TotalSeconds;

        // Record Metrics
        var tags = new KeyValuePair<string, object?>[]
        {
            new("job_name", context.JobDetail.Key.Name),
            new("job_group", context.JobDetail.Key.Group),
            new("status", jobException == null ? "success" : "failure")
        };

        _jobExecutionCounter.Add(1, tags);
        _jobDurationHistogram.Record(duration, tags);

        if (jobException != null)
        {
            _logger.LogError(jobException, ">> Job Failed: {JobKey}", context.JobDetail.Key);
        }
        else
        {
            _logger.LogInformation(">> Job Completed: {JobKey} (Duration: {Duration}s)", context.JobDetail.Key, duration);
        }
        return Task.CompletedTask;
    }
}
