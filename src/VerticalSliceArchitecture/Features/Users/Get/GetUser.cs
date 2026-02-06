using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;

namespace VerticalSliceArchitecture.Features.Users.Get;

public static class GetUser
{
    public record Request(Guid UserId) : IRequest<Response?>;
    public record Response(Guid Id, string Name, string Email, DateTime CreatedAt);

    public class Handler(AppDbContext db) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken ct)
        {
            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null) return null;

            return new Response(user.Id, user.Name, user.Email, user.CreatedAt);
        }
    }
}

public static class GetUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id}", async (Guid id, IMediator mediator) =>
        {
            var response = await mediator.Send(new GetUser.Request(id));
            return response == null ? Results.NotFound() : Results.Ok(response);
        })
        .WithTags("Users");
    }
}
