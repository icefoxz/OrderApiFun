using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using WebUtlLib;

namespace Q_DoApi.Core.Services;

public class BlobService 
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobService()
    {
        _blobServiceClient = new BlobServiceClient(Config.BlobConnectionString());
        _containerName = Config.GetBlobContainerName();
    }

    public async Task<string> UploadFileAsync(Stream fileStream, ILogger log, string contentType = "image/jpeg")
    {
        var id = Guid.NewGuid().ToString();
        log.Event($"Uploading file with ID: {id}");
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