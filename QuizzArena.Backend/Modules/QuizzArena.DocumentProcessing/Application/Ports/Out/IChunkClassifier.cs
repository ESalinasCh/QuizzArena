using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IChunkClassifier
{
    Task<ChunkClassification> ClassifyAsync(string content);
}

public record ChunkClassification(ChunkCategory Category, double Confidence);
