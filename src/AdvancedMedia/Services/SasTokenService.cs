using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace AdvancedMedia.Services;

public class SasTokenService
{
    private readonly BlobContainerClient _containerClient;

    public SasTokenService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StorageConnection");
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient("media");
    }

    public Uri GenerateReadSasUri(string fileName, TimeSpan duration)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("BlobClient must be authorized with Shared Key to generate SAS.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = fileName,
            Resource = "b", // b for Blob
            ExpiresOn = DateTimeOffset.UtcNow.Add(duration)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder);
    }
}
