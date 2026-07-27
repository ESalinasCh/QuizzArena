using Azure.Storage.Blobs;

namespace FFmpeg.Worker.Services;

public class BlobStorageService
{
    private readonly BlobServiceClient _client;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(IConfiguration config, ILogger<BlobStorageService> logger)
    {
        _logger = logger;

        var connectionString = config.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("Connection string 'AzureBlobStorage' was not found.");

        _client = new BlobServiceClient(connectionString);
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