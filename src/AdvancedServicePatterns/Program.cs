using AdvancedServicePatterns.Background;
using AdvancedServicePatterns.Jobs;
using AdvancedServicePatterns.Models;
using AdvancedServicePatterns.Services;
using Microsoft.AspNetCore.Mvc; // For [FromKeyedServices]
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// 1. Advanced DI
// Scrutor scanning remains same (omitted for brevity if unchanged, but keeping for completeness)
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IReportService>()
    .AddClasses(classes => classes.AssignableTo<IReportService>().Where(c => !c.Name.EndsWith("Decorator")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.Decorate<IReportService, LoggingReportDecorator>();

// Strategies (Keyed Services)
builder.Services.AddKeyedScoped<INotificationService, EmailNotificationService>("email");
builder.Services.AddKeyedScoped<INotificationService, SmsNotificationService>("sms");

// 2. Background Processing
builder.Services.AddSingleton<ITaskQueue, ChannelQueue>();
builder.Services.AddHostedService<ReportProcessor>();

// 3. Scheduling
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("CleanupJob");
    q.AddJob<CleanupJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("CleanupTrigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(10) 
            .RepeatForever()));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/reports", async (string name, string type, ITaskQueue queue) =>
{
    var request = new ReportRequest(Guid.NewGuid(), name, type, DateTime.Now);
    await queue.EnqueueAsync(request);
    return Results.Accepted(value: new { Message = "Report queued", RequestId = request.Id });
});

app.MapGet("/api/debug/keyed", ([FromKeyedServices("email")] INotificationService mailer, [FromKeyedServices("sms")] INotificationService texter) =>
{
    return new 
    { 
        EmailService = mailer.GetType().Name, 
        SmsService = texter.GetType().Name 
    };
});

app.Run();
