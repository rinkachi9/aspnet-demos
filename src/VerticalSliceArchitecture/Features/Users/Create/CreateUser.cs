using FluentValidation;
using MediatR;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Users.Shared; // Tier 3
using VerticalSliceArchitecture.Domain.Services; // Tier 4

namespace VerticalSliceArchitecture.Features.Users.Create;

public static class CreateUser
{
    public record Request(string Name, string Email) : IRequest<Response>;
    public record Response(Guid UserId);

    public class Handler(AppDbContext db, UserScoringService scoringService) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken ct)
        {
            // Tier 3: Sharing Validator (Manual usage demo, normally pipeline)
            var validator = new UserValidator(); 
            var validation = validator.Validate(new UserDto(request.Name, request.Email));
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            // Tier 4: Cross-Feature Domain Logic
            var score = scoringService.CalculateInitialScore(request.Email);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            return new Response(user.Id);
        }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}

public static class CreateUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (CreateUser.Request request, IMediator mediator) =>
        {
            var response = await mediator.Send(request);
            return Results.Created($"/users/{response.UserId}", response);
        })
        .WithTags("Users");
    }
}
