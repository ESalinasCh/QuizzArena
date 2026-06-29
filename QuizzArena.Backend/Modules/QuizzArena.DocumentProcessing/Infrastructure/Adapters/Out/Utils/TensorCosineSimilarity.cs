using Pgvector;
using System.Numerics.Tensors;

using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Utils;

internal sealed class TensorCosineSimilarity : ICosineSimilarity
{
    public double CalculateCosineSimilarity(Vector vectorA, Vector vectorB)
    {
        return TensorPrimitives.CosineSimilarity(vectorA.ToArray(), vectorB.ToArray());
    }

    public double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        return TensorPrimitives.CosineSimilarity(vectorA, vectorB);
    }
}
