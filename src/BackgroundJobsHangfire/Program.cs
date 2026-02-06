using BackgroundJobsHangfire.Filters;
using BackgroundJobsHangfire.Services;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// 1. Services
builder.Services.AddTransient<EmailService>();

// 2. Hangfire Configuration
builder.Services.AddHangfire(config =>
{
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options =>
        {
            options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection") 
                // Fallback for demo if conn string missing 
                ?? "Host=localhost;Database=advanced_playground;Username=postgres;Password=postgres");
        });
});

// Add the processing server as IHostedService
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 10;
});

// 3. Register Global Filters (via DI or manually)
// Note: Hangfire filters are often singletons or instantiated per use. 
// For simplicity in Program.cs, we register a global filter using GlobalJobFilters.Headers
// But to use DI logger, we need to resolve it. 
// A common pattern is to let the DI container handle filter lifetimes or pass instance.

var app = builder.Build();

// Resolve Logger for the filter
var logger = app.Services.GetRequiredService<ILogger<JobLoggingFilter>>();
GlobalJobFilters.Filters.Add(new JobLoggingFilter(logger));

// 4. Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Advanced Playground Jobs",
    // Authorization = new [] { new MyAuthorizationFilter() } // Implement for prod
});

// 5. API Triggers
app.MapPost("/jobs/fire-and-forget", (IBackgroundJobClient jobClient, string email) =>
{
    var jobId = jobClient.Enqueue<EmailService>(x => x.SendWelcomeEmail(email));
    return Results.Ok(new { JobId = jobId, Status = "Enqueued" });
});

app.MapPost("/jobs/delayed", (IBackgroundJobClient jobClient, string email) =>
{
    var jobId = jobClient.Schedule<EmailService>(x => x.SendWelcomeEmail(email), TimeSpan.FromMinutes(1));
    return Results.Ok(new { JobId = jobId, Status = "Scheduled (Delayed 1m)" });
});

app.MapPost("/jobs/recurring", (IRecurringJobManager recurringJobManager) =>
{
    recurringJobManager.AddOrUpdate<EmailService>("monthly-report", x => x.ProcessMonthlyReport(), Cron.Monthly);
    return Results.Ok("Recurring job 'monthly-report' updated.");
});

// 6. Manual Trigger via API for Recurring
app.MapPost("/jobs/recurring/trigger", (IRecurringJobManager recurringJobManager) => 
{
    recurringJobManager.Trigger("monthly-report");
    return Results.Ok("Triggered monthly-report");
});

app.Run();
