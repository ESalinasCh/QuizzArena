using Azure.Identity;
using Azure.Storage.Blobs;

namespace FFmpeg.Worker.Services;

public class BlobStorageService
{
    private readonly BlobServiceClient _client;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(IConfiguration config, ILogger<BlobStorageService> logger)
    {
        _logger = logger;

        var accountUrl = config["AzureStorage:AccountUrl"]
            ?? throw new InvalidOperationException("AzureStorage:AccountUrl no está configurado.");

        // DefaultAzureCredential resuelve automáticamente según dónde corra:
        // - En tu PC: usa la sesión de "az login" (AzureCliCredential)
        // - En Container Apps: usa la identidad administrada del recurso (ManagedIdentityCredential)
        // Mismo código, cero branching por entorno.
        _client = new BlobServiceClient(new Uri(accountUrl), new DefaultAzureCredential());
    }

    public async Task<string> UploadAsync(string localFilePath, string containerName, string blobName, CancellationToken ct)
    {
        var container = _client.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobClient = container.GetBlobClient(blobName);
        _logger.LogInformation("Uploading {Local} -> {Container}/{Blob}", localFilePath, containerName, blobName);

        await using var stream = File.OpenRead(localFilePath);
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);

        return blobClient.Uri.ToString();
    }
}