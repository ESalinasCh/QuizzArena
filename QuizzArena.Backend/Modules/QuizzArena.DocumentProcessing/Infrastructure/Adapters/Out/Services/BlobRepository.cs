using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

public class BlobRepository(BlobServiceClient blobServiceClient) : IBlobRepository
{
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName.ToLower());

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);

        return blobClient.Uri.AbsoluteUri;
    }

    public async Task<string> UploadTextAsync(string text, string fileName, string containerName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName.ToLower());

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(text)), overwrite: true);

        return blobClient.Uri.AbsoluteUri;
    }
}
