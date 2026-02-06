namespace VerticalSliceArchitecture.Infrastructure;

// Replaces MockCloudStorageClient
public class LocalStorageClient : ICloudStorageClient
{
    private readonly string _basePath;
    private readonly ILogger<LocalStorageClient> _logger;

    public LocalStorageClient(IConfiguration configuration, ILogger<LocalStorageClient> logger)
    {
        _basePath = configuration["Storage:BasePath"] ?? "temp/storage";
        _logger = logger;
        
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadAsJsonAsync(string fileName, object data, DateTime expiresAt, CancellationToken ct)
    {
        var filePath = Path.Combine(_basePath, fileName);
        var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync(filePath, json, ct);
        _logger.LogInformation("Saved file to Local Storage at: {Path}", filePath);

        // In real world, this would be a public URL or SAS token
        return $"file://{filePath}";
    }

    // New helper method for generic file uploads (IFormFile etc)
    public async Task<string> UploadFileAsync(string fileName, Stream stream, CancellationToken ct)
    {
        var filePath = Path.Combine(_basePath, fileName);
        
        using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream, ct);
        
        _logger.LogInformation("Saved binary file to Local Storage at: {Path}", filePath);
        return $"file://{filePath}";
    }
}
