using AdvancedMessaging.Contracts;
using AdvancedMessaging.Sagas;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? "Host=localhost;Port=5433;Database=advanced_playground;Username=admin;Password=password";

builder.Services.AddDbContext<OrderSagaDbContext>(options =>
{
    options.UseNpgsql(connectionString, m => m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));
});

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
     .EntityFrameworkRepository(r =>
     {
         r.ExistingDbContext<OrderSagaDbContext>();
         r.UsePostgres();
     });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/order", async (IPublishEndpoint publisher, SubmitOrder order) =>
{
    await publisher.Publish(order);
    return Results.Accepted();
});

// Auto-migrate for demo purposes
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderSagaDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
