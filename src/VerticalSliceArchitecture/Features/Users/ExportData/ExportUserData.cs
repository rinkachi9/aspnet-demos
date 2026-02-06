using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Infrastructure;

namespace VerticalSliceArchitecture.Features.Users.ExportData;

// The Vertical Slice: Everything in one file (or folder)
public static class ExportUserData
{
    // The Request (Input)
    public record Request(Guid UserId) : IRequest<Response>;

    // The Response (Output)
    public record Response(string DownloadUrl, DateTime ExpiresAt);

    // The Handler (Business Logic)
    public class Handler(
        AppDbContext dbContext,
        ICloudStorageClient storageClient,
        IEmailSender emailSender,
        ILogger<Handler> logger)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken ct = default)
        {
            logger.LogInformation("Starting export for User {UserId}", request.UserId);

            // 1. Get user data (Aggregate)
            var user = await dbContext.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null)
            {
                // In real app: throw NotFoundException
                logger.LogWarning("User {UserId} not found", request.UserId);
                 throw new KeyNotFoundException($"User {request.UserId} not found");
            }

            // 2. Generate export data (DTO projection)
            var exportData = new
            {
                user.Email,
                user.Name,
                user.CreatedAt,
                Orders = user.Orders.Select(o => new { o.Id, o.Total, o.Date }),
                user.Preferences
            };

            // 3. Upload to cloud storage
            var fileName = $"user-data-{user.Id}-{DateTime.UtcNow:yyyyMMdd}.json";
            var expiresAtUtc = DateTime.UtcNow.AddDays(7);

            var downloadUrl = await storageClient.UploadAsJsonAsync(
                fileName,
                exportData,
                expiresAtUtc,
                ct);

            // 4. Send email notification
            await emailSender.SendDataExportEmailAsync(user.Email, downloadUrl, ct);

            return new Response(downloadUrl, expiresAtUtc);
        }
    }

    // The Validator (Input Validation)
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.UserId).NotEmpty();
        }
    }
}
