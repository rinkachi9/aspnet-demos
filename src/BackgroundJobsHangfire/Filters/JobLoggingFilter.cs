using Hangfire.Server;
using Hangfire.Common;
using Hangfire.States;

namespace BackgroundJobsHangfire.Filters;

public class JobLoggingFilter : IServerFilter, IElectStateFilter
{
    private readonly ILogger<JobLoggingFilter> _logger;

    public JobLoggingFilter(ILogger<JobLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnPerforming(PerformingContext context)
    {
        _logger.LogInformation(">> [Hangfire] Starting Job: {JobId} ({Type}.{Method})", 
            context.BackgroundJob.Id, 
            context.BackgroundJob.Job.Type.Name, 
            context.BackgroundJob.Job.Method.Name);
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Exception != null)
        {
            _logger.LogError(context.Exception, ">> [Hangfire] Job Failed: {JobId}", context.BackgroundJob.Id);
        }
        else
        {
            _logger.LogInformation(">> [Hangfire] Job Completed: {JobId}", context.BackgroundJob.Id);
        }
    }

    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState is FailedState failedState)
        {
            _logger.LogWarning(">> [Hangfire] Job {JobId} election to FAILED state. Reason: {Reason}", 
                context.BackgroundJob.Id, 
                failedState.Exception.Message);
        }
    }
}
