using Asp.Versioning.Builder;
using CleanArchitecture.Application.Orders.Commands.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/orders")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(1, 0)
            .WithTags("Orders")
            .WithOpenApi();

        group.MapPost("/", async (ISender sender, [FromBody] CreateOrderCommand command) =>
        {
            var orderId = await sender.Send(command);
            return Results.Created($"/api/v1/orders/{orderId}", new { Id = orderId });
        })
        .WithName("CreateOrder")
        .Produces(201)
        .ProducesValidationProblem();
    }
}
