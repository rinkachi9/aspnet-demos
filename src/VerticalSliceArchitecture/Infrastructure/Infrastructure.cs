namespace VerticalSliceArchitecture.Infrastructure;

public interface ICloudStorageClient
{
    Task<string> UploadAsJsonAsync(string fileName, object data, DateTime expiresAt, CancellationToken ct);
    Task<string> UploadFileAsync(string fileName, Stream stream, CancellationToken ct);
}

public class MockCloudStorageClient : ICloudStorageClient
{
    public Task<string> UploadAsJsonAsync(string fileName, object data, DateTime expiresAt, CancellationToken ct)
    {
        // Simulate upload
        return Task.FromResult($"https://cloud-storage.demo/{fileName}?expires={expiresAt:O}");
    }

    public Task<string> UploadFileAsync(string fileName, Stream stream, CancellationToken ct)
    {
        return Task.FromResult($"https://cloud-storage.demo/{fileName}");
    }
}

public interface IEmailSender
{
    Task SendDataExportEmailAsync(string email, string downloadUrl, CancellationToken ct);
}

public class MockEmailSender : IEmailSender
{
    private readonly ILogger<MockEmailSender> _logger;

    public MockEmailSender(ILogger<MockEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendDataExportEmailAsync(string email, string downloadUrl, CancellationToken ct)
    {
        _logger.LogInformation("Sending export email to {Email}. Link: {Url}", email, downloadUrl);
        return Task.CompletedTask;
    }
}
