using BackgroundJobsQuartz.Extensions;
using BackgroundJobsQuartz.Listeners;
using Quartz;
using Quartz.AspNetCore;
using Quartz.Impl.Matchers;

using BackgroundJobsQuartz.Options;

var builder = WebApplication.CreateBuilder(args);

// 0. Configuration
builder.Services.Configure<ReportJobOptions>(builder.Configuration.GetSection(ReportJobOptions.SectionName));

// 1. Quartz Configuration
builder.Services.AddQuartz(q =>
{
    // Register Global Listener
    q.AddJobListener<GlobalJobListener>();

    // Auto-scan current assembly for [QuartzJob] + Apply Config Overrides
    q.AddQuartzJobsFromAssembly(builder.Configuration, typeof(Program).Assembly);
});

builder.Services.AddQuartzServer(options =>
{
    options.WaitForJobsToComplete = true; // Graceful shutdown
});

var app = builder.Build();

// 2. API to Trigger Jobs Manually
app.MapPost("/jobs/{group}/{name}/trigger", async (string group, string name, ISchedulerFactory schedulerFactory) =>
{
    var scheduler = await schedulerFactory.GetScheduler();
    var jobKey = new JobKey(name, group);

    if (await scheduler.CheckExists(jobKey))
    {
        await scheduler.TriggerJob(jobKey);
        return Results.Ok($"Triggered job: {group}.{name}");
    }

    return Results.NotFound($"Job not found: {group}.{name}");
});

// List all jobs
app.MapGet("/jobs", async (ISchedulerFactory schedulerFactory) =>
{
    var scheduler = await schedulerFactory.GetScheduler();
    var groups = await scheduler.GetJobGroupNames();
    
    var result = new List<object>();

    foreach (var group in groups)
    {
        var keys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));
        foreach (var key in keys)
        {
            var detail = await scheduler.GetJobDetail(key);
            var triggers = await scheduler.GetTriggersOfJob(key);
            
            result.Add(new 
            {
                Group = group,
                Name = key.Name,
                Type = detail?.JobType.Name,
                NextFireTime = triggers.FirstOrDefault()?.GetNextFireTimeUtc()
            });
        }
    }

    return Results.Ok(result);
});

app.Run();
