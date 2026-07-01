using System.Numerics.Tensors;

namespace QuizzArena.DocumentProcessing.Application.Helpers;

internal static class SemanticChunker
{
    private const float ChunkSimilarityThreshold = 0.45f;
    private const int MinChunkWords = 80;
    private const int MaxChunkWords = 400;

    internal static List<string> GenerateChunk(IReadOnlyList<string> sentences, IReadOnlyList<float[]> embeddings)
    {
        if (sentences.Count != embeddings.Count)
        {
            throw new ArgumentException(
                $"Sentences ({sentences.Count}) and embeddings ({embeddings.Count}) must be 1:1 and in the same order.");
        }

        List<string> chunks = [];
        if (sentences.Count == 0)
        {
            return chunks;
        }

        List<string> currentSentences = [sentences[0]];
        int currentWordCount = WordCount(sentences[0]);

        // Walk each adjacent pair. similarity[i] compares sentence i with sentence i+1.
        for (int i = 0; i < sentences.Count - 1; i++)
        {
            float similarity = TensorPrimitives.CosineSimilarity(embeddings[i], embeddings[i + 1]);
            string nextSentence = sentences[i + 1];
            int nextWordCount = WordCount(nextSentence);

            bool shouldSplit = false;

            if (similarity < ChunkSimilarityThreshold && currentWordCount >= MinChunkWords)
            {
                shouldSplit = true;
            }

            if (currentWordCount + nextWordCount > MaxChunkWords)
            {
                shouldSplit = true;
            }

            if (shouldSplit)
            {
                chunks.Add(string.Join(' ', currentSentences));
                currentSentences = [nextSentence];
                currentWordCount = nextWordCount;
            }
            else
            {
                currentSentences.Add(nextSentence);
                currentWordCount += nextWordCount;
            }
        }

        // Flush the trailing chunk.
        if (currentSentences.Count > 0)
        {
            chunks.Add(string.Join(' ', currentSentences));
        }

        return chunks;
    }

    private static int WordCount(string sentence) =>
        sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

}
