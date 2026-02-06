using DomainEventsOutbox.Api.Data;
using DomainEventsOutbox.Api.Domain;
using DomainEventsOutbox.Api.Infrastructure;
using DomainEventsOutbox.Api.Jobs;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// 1. Database & Interceptor
builder.Services.AddSingleton<OutboxInterceptor>(); // Singleton is fine for interceptor if stateless, otherwise Scoped
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("OutboxDb"));

// 2. MassTransit (RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context)); // Demo: InMemory transport
});

// 3. Quartz (Outbox Processor)
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("OutboxProcessor");
    q.AddJob<OutboxProcessorJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("OutboxTrigger")
        .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever()));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// 4. OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. Endpoint to Trigger Flow
app.MapPost("/users", async (AppDbContext db, string email, string name) =>
{
    var user = new User(email, name);
    db.Users.Add(user);
    
    // Check: DB transaction saves both User AND OutboxMessage
    await db.SaveChangesAsync();

    return Results.Ok(new { user.Id, Message = "User created. Event saved to Outbox." });
});

app.MapGet("/outbox", async (AppDbContext db) =>
{
    return await db.OutboxMessages.ToListAsync();
});

app.Run();
