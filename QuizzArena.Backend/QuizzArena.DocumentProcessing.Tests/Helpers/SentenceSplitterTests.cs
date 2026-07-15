using FluentAssertions;
using QuizzArena.DocumentProcessing.Application.Helpers;

namespace QuizzArena.DocumentProcessing.Tests.Helpers;

public class SentenceSplitterTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SplitIntoSentences_EmptyOrWhitespace_ReturnsEmpty(string? text)
    {
        List<string> result = SentenceSplitter.SplitIntoSentences(text!, 15);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SplitIntoSentences_SplitsOnSentenceEndingPunctuation()
    {
        const string text = "Hello world. How are you? I am fine!";

        List<string> result = SentenceSplitter.SplitIntoSentences(text, 15, minWords: 1);

        result.Should().Equal("Hello world.", "How are you?", "I am fine!");
    }

    [Fact]
    public void SplitIntoSentences_FlushesAtWordCapWhenNoPunctuation()
    {
        string text = string.Join(' ', Enumerable.Range(1, 17).Select(i => $"w{i}"));

        List<string> result = SentenceSplitter.SplitIntoSentences(text, 15, minWords: 1);

        result.Should().HaveCount(2);
        result[0].Split(' ').Should().HaveCount(15);
        result[1].Split(' ').Should().HaveCount(2);
    }

    [Fact]
    public void SplitIntoSentences_FlushesTrailingWordsWithoutPunctuation()
    {
        const string text = "First sentence. Trailing words with no period";

        List<string> result = SentenceSplitter.SplitIntoSentences(text, 15, minWords: 1);

        result.Should().Equal("First sentence.", "Trailing words with no period");
    }

    [Fact]
    public void SplitIntoSentences_CollapsesExtraWhitespace()
    {
        const string text = "Too    many   spaces.";

        List<string> result = SentenceSplitter.SplitIntoSentences(text, 15, minWords: 1);

        result.Should().ContainSingle().Which.Should().Be("Too many spaces.");
    }

    [Fact]
    public void SplitIntoSentences_MergesShortSentenceForwardIntoNextSentence()
    {
        const string text = "And set that value. Okay. So the first method we will implement is append.";

        List<string> result = SentenceSplitter.SplitIntoSentences(text, 15, minWords: 4);

        result.Should().Equal(
            "And set that value.",
            "Okay. So the first method we will implement is append."
        );
    }

    [Fact]
    public void SplitIntoSentences_MergesTrailingShortSentenceIntoPreviousSentence()
    {
        const string text = "So the first method we will implement is append. Okay.";

        List<string> result = SentenceSplitter.SplitIntoSentences(text, 15, minWords: 4);

        result.Should().Equal("So the first method we will implement is append. Okay.");
    }

    [Fact]
    public void SplitIntoSentences_WholeTextBelowMinWords_ReturnsItAsIs()
    {
        const string text = "Okay.";

        List<string> result = SentenceSplitter.SplitIntoSentences(text, 15, minWords: 4);

        result.Should().Equal("Okay.");
    }

    [Fact]
    public void SplitIntoSentences_DefaultMinWords_MergesSingleWordFragment()
    {
        const string text = "And set that value. Okay. So the first method we will implement is append.";

        List<string> result = SentenceSplitter.SplitIntoSentences(text, 15);

        result.Should().NotContain("Okay.");
    }
}
