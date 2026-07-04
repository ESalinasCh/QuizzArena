using System.Numerics.Tensors;
using Pgvector;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Utils;

internal static class TensorCosineSimilarity
{
    public static double CalculateCosineSimilarity(Vector vectorA, Vector vectorB)
    {
        return TensorPrimitives.CosineSimilarity(vectorA.ToArray(), vectorB.ToArray());
    }

    public static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        return TensorPrimitives.CosineSimilarity(vectorA, vectorB);
    }
}
