namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IStorageServiceRepository
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName);
    Task<string> UploadTextAsync(string text, string fileName, string containerName);
    Task<string> DownloadTextAsync(string fileUrl);
}
