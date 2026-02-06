using AdvancedMassTransit.Activities;
using AdvancedMassTransit.Consumers;
using AdvancedMassTransit.Contracts;
using AdvancedMassTransit.Data;
using AdvancedMassTransit.Sagas;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMassTransit(x =>
{
    // 1. SAGA
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
     .EntityFrameworkRepository(r =>
     {
         r.ConcurrencyMode = ConcurrencyMode.Optimistic;
         r.UsePostgres();
         r.ExistingDbContext<AppDbContext>();
     });

    // 2. COURIER ACTIVITIES (Routing Slip)
    x.AddActivitiesFromNamespaceContaining<BlockFundsActivity>();

    // 3. OUTBOX
    x.AddEntityFrameworkOutbox<AppDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    // 4. RABBITMQ with Advanced Config
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => 
        {
            h.Username("guest");
            h.Password("guest");
        });

        // GLOBAL RETRY POLICY
        cfg.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(500)));
        
        // SCHEDULING (Requires RabbitMQ Delayed Message Plugin or Quartz)
        cfg.UseDelayedMessageScheduler();

        cfg.ConfigureEndpoints(context);
    });

    x.AddRider(rider =>
    {
        rider.AddConsumer<TelemetryConsumer>();
        rider.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092");
            k.TopicEndpoint<OrderTelemetry>("order-telemetry", "telemetry-group", e =>
            {
                e.ConfigureConsumer<TelemetryConsumer>(context);
                e.CreateIfMissing(t => t.NumPartitions = 1);
            });
        });
    });
});

var app = builder.Build();

// ... (Existing endpoints)

// ROUTING SLIP DEMO
app.MapPost("/order/courier/{orderId}", async (Guid orderId, IBus bus) =>
{
    var builder = new RoutingSlipBuilder(NewId.NextGuid());

    // 1. Block Funds
    builder.AddActivity("BlockFunds", new Uri("queue:BlockFunds_execute"), new BlockFundsArguments(orderId, 150.00m, "ACC-123"));

    // 2. Allocate Inventory
    // Note: If quantity > 100, this Activity throws, triggering BlockFunds.Compensate()
    builder.AddActivity("AllocateInventory", new Uri("queue:AllocateInventory_execute"), new AllocateInventoryArguments(orderId, "SKU-999", 5)); 

    var routingSlip = builder.Build();
    
    await bus.Execute(routingSlip);
    return Results.Accepted("Routing Slip Started");
});

app.MapPost("/order", async (IPublishEndpoint publishEndpoint, AppDbContext db) =>
{
    var orderId = Guid.NewGuid();
    await publishEndpoint.Publish(new SubmitOrder(orderId, "CUST-001", 99.00m));
    await db.SaveChangesAsync();
    return Results.Accepted($"/order/{orderId}", new { OrderId = orderId });
});

app.MapGet("/order/{id}", async (Guid id, IRequestClient<CheckOrderStatus> client) =>
{
    var response = await client.GetResponse<OrderStatusResult>(new CheckOrderStatus(id));
    return Results.Ok(response.Message);
});

app.MapPost("/order/{id}/validate", async (Guid id, IPublishEndpoint p, AppDbContext db) =>
{
    await p.Publish(new OrderValidated(id, true));
    await db.SaveChangesAsync();
    return Results.Ok("Validated");
});

app.MapPost("/telemetry/{id}", async (Guid id, ITopicProducer<OrderTelemetry> producer) =>
{
    await producer.Produce(new OrderTelemetry(id, "Viewed", 45.5));
    return Results.Ok("Telemetry Sent to Kafka");
});

app.Run();
