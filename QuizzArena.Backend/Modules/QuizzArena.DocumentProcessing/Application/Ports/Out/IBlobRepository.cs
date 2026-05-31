namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IBlobRepository
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName);
}
