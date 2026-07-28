namespace QuizzArena.DocumentProcessing.Application.Helpers;

internal static class TextChunker
{
    public static List<string> ChunkList(
        List<string> sentences,
        int maxChunkSize = 10000,
        string separator = " "
    )
    {
        var chunks = new List<string>();
        var currentChunk = new List<string>();
        var currentLength = 0;
        var sepLength = separator.Length;

        foreach (var sentence in sentences)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                continue;
            }

            var trimmedSentence = sentence.Trim();
            var currentSeparatorLength = currentChunk.Count > 0 ? sepLength : 0;

            if (currentLength + trimmedSentence.Length + currentSeparatorLength > maxChunkSize)
            {
                if (currentChunk.Count > 0)
                {
                    chunks.Add(string.Join(separator, currentChunk));
                    currentChunk.Clear();
                    currentLength = 0;
                }
            }

            currentChunk.Add(trimmedSentence);

            // Recalculamos la longitud acumulada incluyendo el separador
            var isFirstInChunk = currentChunk.Count == 1;
            currentLength += trimmedSentence.Length + (isFirstInChunk ? 0 : sepLength);
        }

        if (currentChunk.Count > 0)
        {
            chunks.Add(string.Join(separator, currentChunk));
        }

        return chunks;
    }
}
