namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IEmbeddingService
{
    public Task<float[]> GenerateSingleEmbeddingAsync(string model, string prompt);
    public Task<float[][]> GenerateMultipleEmbeddingsAsync(string model, string[] prompt);
    Task<IReadOnlyList<float[]>> EmbedInBatchesAsync(IReadOnlyList<string> sentences);
}
