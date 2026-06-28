using Pgvector;
using System.Numerics.Tensors;

using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Utils;

internal class TensorCosineSimilarity : ICosineSimilarity
{
    public double CalculateCosineSimilarity(Vector vectorA, Vector vectorB)
    {
        ReadOnlySpan<float> a = vectorA.ToArray();
        ReadOnlySpan<float> b = vectorB.ToArray();
        return TensorPrimitives.CosineSimilarity(a, b);
    }
}
