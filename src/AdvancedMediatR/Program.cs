using AdvancedMediatR.Behaviors;
using AdvancedMediatR.Features.Orders;
using FluentValidation;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR & Behaviors
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/orders", async (ISender sender, CreateOrderCommand command) => {
    var id = await sender.Send(command);
    return Results.Created($"/orders/{id}", id);
})
.WithTags("Orders")
.WithOpenApi();

app.Run();
