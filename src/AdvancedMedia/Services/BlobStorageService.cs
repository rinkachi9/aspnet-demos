using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace AdvancedMedia.Services;

public class BlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StorageConnection");
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient("media");
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadFileChunkedAsync(Stream content, string fileName, string contentType)
    {
        var blobClient = _containerClient.GetBlockBlobClient(fileName);
        
        // Use Block Blobs for reliable upload of large files
        // In a real scenario, the content stream might be very large
        var options = new BlobUploadOptions
        {
             HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(content, options);
        return blobClient.Uri.ToString();
    }

    public async Task<Stream?> GetFileStreamAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        if (!await blobClient.ExistsAsync()) return null;

        var downloadInfo = await blobClient.DownloadAsync();
        return downloadInfo.Value.Content;
    }

    public BlockBlobClient GetBlockBlobClient(string fileName)
    {
        return _containerClient.GetBlockBlobClient(fileName);
    }
}
