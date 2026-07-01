namespace QuizzArena.DocumentProcessing.Application.Helpers;

internal static class SentenceSplitter
{
    private const int MaxSentenceWords = 15;

    public static List<string> SplitIntoSentences(string text)
    {
        List<string> sentences = [];

        if (string.IsNullOrWhiteSpace(text))
        {
            return sentences;
        }

        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        List<string> currentWords = [];

        foreach (string word in words)
        {
            currentWords.Add(word);

            bool endsWithPunc = word.EndsWith('.') || word.EndsWith('?') || word.EndsWith('!');

            if (endsWithPunc || currentWords.Count >= MaxSentenceWords)
            {
                sentences.Add(string.Join(' ', currentWords));
                currentWords.Clear();
            }
        }

        if (currentWords.Count > 0)
        {
            sentences.Add(string.Join(' ', currentWords));
        }

        return sentences;
    }
}
