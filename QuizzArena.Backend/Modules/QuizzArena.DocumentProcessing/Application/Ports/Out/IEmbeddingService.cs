namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public sealed record EmbeddingBatchResult(float[][] Embeddings, IReadOnlySet<int> SkippedIndices);

public interface IEmbeddingService
{
    public Task<float[]> GenerateSingleEmbeddingAsync(string model, string prompt);
    public Task<EmbeddingBatchResult> GenerateMultipleEmbeddingsAsync(string model, string[] prompts, int? batchSize = 32);
}
