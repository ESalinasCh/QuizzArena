namespace QuizzArena.DocumentProcessing.Application.Helpers;

internal static class SentenceSplitter
{
    public static List<string> SplitIntoSentences(string text, int maxWords, int minWords = 4)
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

            if ((endsWithPunc && currentWords.Count >= minWords) || currentWords.Count >= maxWords)
            {
                sentences.Add(string.Join(' ', currentWords));
                currentWords.Clear();
            }
        }

        if (currentWords.Count > 0)
        {
            if (currentWords.Count < minWords && sentences.Count > 0)
            {
                sentences[^1] = $"{sentences[^1]} {string.Join(' ', currentWords)}";
            }
            else
            {
                sentences.Add(string.Join(' ', currentWords));
            }
        }

        return sentences;
    }
}
