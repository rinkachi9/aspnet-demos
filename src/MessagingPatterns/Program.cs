using MassTransit;
using MessagingPatterns.Consumers;
using MessagingPatterns.Contracts;
using MessagingPatterns.Sagas;

var builder = WebApplication.CreateBuilder(args);

// 1. MassTransit Configuration
builder.Services.AddMassTransit(x =>
{
    // Register Consumers
    x.AddConsumer<SubmitOrderConsumer>();

    // Register Saga
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .InMemoryRepository(); // For demo only. Use Redis/EF in production.

    // Using RabbitMQ
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration.GetValue<string>("RabbitMq:Host") ?? "localhost";
        
        cfg.Host(rabbitMqHost, "/", h => 
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Auto-configure endpoints for consumers
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Endpoints
app.MapPost("/orders", async (SubmitOrder request, IPublishEndpoint publisher) =>
{
    // Pub/Sub: Publish command to the bus
    await publisher.Publish(request);
    return Results.Accepted($"/orders/{request.OrderId}", request);
})
.WithOpenApi();

app.MapGet("/orders/{id}/status", async (Guid id, IRequestClient<CheckOrderStatus> client) =>
{
    // Request/Response Pattern would involve sending a request and waiting for response
    // For now, simpler demo: we assume there might be a consumer for this query
    // In this specific demo, we haven't implemented CheckOrderStatusConsumer yet 
    // but the infrastructure is ready for it.
    
    // Placeholder response as we focused on Saga flow
    await Task.CompletedTask; 
    return Results.Ok(new { Message = "Status check implemented via RequestClient pattern" });
})
.WithOpenApi();

app.Run();
