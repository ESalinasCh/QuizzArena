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

        var connectionString = config["ConnectionStrings:AzureBlobStorage"];

        if (!string.IsNullOrEmpty(connectionString))
        {
            // Local: Azurite via connection string (HTTP, sin credenciales Azure AD)
            _client = new BlobServiceClient(connectionString);
        }
        else
        {
            // Producción: Blob Storage real via managed identity (HTTPS)
            var accountUrl = config["AzureStorage:AccountUrl"]
                ?? throw new InvalidOperationException("Se requiere 'AzureStorage:AccountUrl' cuando no hay connection string configurado.");

            _client = new BlobServiceClient(
                new Uri(accountUrl),
                new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeVisualStudioCredential = true
                }));
        }
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
