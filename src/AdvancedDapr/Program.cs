using AdvancedDapr.Actors;
using AdvancedDapr.Workflows;
using Dapr.Workflow;

var builder = WebApplication.CreateBuilder(args);

// Add Dapr & Actors
builder.Services.AddControllers().AddDapr();
builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<SmartDeviceActor>();
});

// Add Dapr Workflow
builder.Services.AddDaprWorkflow(options =>
{
    options.RegisterWorkflow<OrderWorkflow>();
    options.RegisterActivity<VerifyOrderActivity>();
    options.RegisterActivity<ApproveOrderActivity>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CloudEvents and Subscribers
app.UseCloudEvents();
app.MapSubscribeHandler();
app.MapControllers(); // For Dapr Controllers
app.MapActorsHandlers(); // For Actors

app.MapGet("/", () => "Advanced Dapr Service Running");

app.Run();
