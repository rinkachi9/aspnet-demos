using System.Reflection;
using BackgroundJobsQuartz.Attributes;
using Quartz;

namespace BackgroundJobsQuartz.Extensions;

public static class QuartzExtensions
{
    public static void AddQuartzJobsFromAssembly(this IServiceCollectionQuartzConfigurator q, IConfiguration configuration, Assembly assembly)
    {
        var jobTypes = assembly.GetTypes()
            .Where(t => typeof(IJob).IsAssignableFrom(t) && !t.IsAbstract && t.GetCustomAttribute<QuartzJobAttribute>() != null)
            .ToList();

        foreach (var jobType in jobTypes)
        {
            var attr = jobType.GetCustomAttribute<QuartzJobAttribute>()!;
            var identity = attr.Identity ?? jobType.Name;
            var group = attr.Group ?? "Default";
            
            // Configuration Lookup
            var configKey = $"Quartz:Jobs:{identity}:Cron";
            var cronFromConfig = configuration[configKey];
            var cronExpression = !string.IsNullOrWhiteSpace(cronFromConfig) ? cronFromConfig : attr.CronExpression;

            var jobKey = new JobKey(identity, group);

            q.AddJob(jobType, jobKey, opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{identity}-trigger", group)
                .WithCronSchedule(cronExpression));

            var source = !string.IsNullOrWhiteSpace(cronFromConfig) ? "Config" : "Attribute";
            Console.WriteLine($"[Quartz] Registered job: {identity} with cron: {cronExpression} (Source: {source})");
        }
    }
}
