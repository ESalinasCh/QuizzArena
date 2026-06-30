namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface ICosineSimilarity
{
    double CalculateCosineSimilarity(Pgvector.Vector vectorA, Pgvector.Vector vectorB);
    double CalculateCosineSimilarity(float[] vectorA, float[] vectorB);
}
