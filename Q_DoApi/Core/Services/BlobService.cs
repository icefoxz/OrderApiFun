using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Q_DoApi.Core.Services;

public class BlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobService(BlobServiceClient blobServiceClient, string containerName)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string contentType = "image/jpeg")
    {
        var id = Guid.NewGuid().ToString();
        var blobClient = _blobServiceClient.GetBlobContainerClient(_containerName).GetBlobClient(id);

        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        });

        return id;
    }
}