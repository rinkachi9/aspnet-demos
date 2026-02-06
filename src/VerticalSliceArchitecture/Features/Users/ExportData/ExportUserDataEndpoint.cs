using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace VerticalSliceArchitecture.Features.Users.ExportData;

public static class ExportUserDataEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId}/export", async (
            Guid userId,
            [FromServices] IMediator mediator) =>
        {
            try 
            {
                var response = await mediator.Send(new ExportUserData.Request(userId));
                return Results.Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithTags("Users")
        .WithName("ExportUserData");
    }
}
