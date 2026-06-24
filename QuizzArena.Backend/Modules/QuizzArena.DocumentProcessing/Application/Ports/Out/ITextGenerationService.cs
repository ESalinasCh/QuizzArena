namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface ITextGenerationService
{
    public Task<T> GenerateAsync<T>(string model, string prompt);
}
