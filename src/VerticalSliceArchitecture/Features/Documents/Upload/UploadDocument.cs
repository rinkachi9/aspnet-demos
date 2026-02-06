using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.Documents.Upload;

public static class UploadDocument
{
    // Important: IFormFile cannot be in a record if serialized as JSON, but for MediatR it's fine
    // provided we map it correctly in the endpoint.
    public class Request : IRequest<Response>
    {
        public Guid UserId { get; set; }
        public IFormFile File { get; set; } = default!;
    }
    
    public record Response(string FileUrl);

    public class Handler(
        ICloudStorageClient storage,
        ILogger<Handler> logger) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken ct)
        {
            var fileName = $"{request.UserId}/{Guid.NewGuid()}_{request.File.FileName}";
            
            // Ensure directory exists for user (optional, handled by storage client mostly)
            
            using var stream = request.File.OpenReadStream();
            var url = await storage.UploadFileAsync(fileName, stream, ct);
            
            logger.LogInformation("Uploaded document for User {UserId}", request.UserId);
            
            return new Response(url);
        }
    }
    
    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.File).NotNull();
        }
    }
}

public static class UploadDocumentEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // Minimal API doesn't auto-bind IFormFile in complex objects easily without [FromForm]
        // But for MediatR we construct the request manually.
        app.MapPost("/users/{userId}/documents", async (
            Guid userId,
            IFormFile file,
            IMediator mediator) =>
        {
            var request = new UploadDocument.Request { UserId = userId, File = file };
            var response = await mediator.Send(request);
            return Results.Ok(response);
        })
        .DisableAntiforgery() // Important for file uploads sometimes
        .WithTags("Documents");
    }
}
