namespace QuizzArena.DocumentProcessing.Application.Helpers;

internal static class TextChunker
{
    public static List<string> ChunkSentences(List<string> sentences, int maxChunkSize = 10000)
    {
        var chunks = new List<string>();
        var currentChunk = new List<string>();
        var currentLength = 0;

        foreach (var sentence in sentences)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                continue;
            }

            var trimmedSentence = sentence.Trim();
            var separatorLength = currentChunk.Count > 0 ? 1 : 0;

            if (currentLength + trimmedSentence.Length + separatorLength > maxChunkSize)
            {
                if (currentChunk.Count > 0)
                {
                    chunks.Add(string.Join(" ", currentChunk));
                    currentChunk.Clear();
                    currentLength = 0;
                }
            }

            currentChunk.Add(trimmedSentence);
            currentLength += trimmedSentence.Length + (currentChunk.Count > 1 ? 1 : 0);
        }

        if (currentChunk.Count > 0)
        {
            chunks.Add(string.Join(" ", currentChunk));
        }

        return chunks;
    }
}
