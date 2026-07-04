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
        List<string> result = SentenceSplitter.SplitIntoSentences(text!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SplitIntoSentences_SplitsOnSentenceEndingPunctuation()
    {
        const string text = "Hello world. How are you? I am fine!";

        List<string> result = SentenceSplitter.SplitIntoSentences(text);

        result.Should().Equal("Hello world.", "How are you?", "I am fine!");
    }

    [Fact]
    public void SplitIntoSentences_FlushesAtWordCapWhenNoPunctuation()
    {
        string text = string.Join(' ', Enumerable.Range(1, 17).Select(i => $"w{i}"));

        List<string> result = SentenceSplitter.SplitIntoSentences(text);

        result.Should().HaveCount(2);
        result[0].Split(' ').Should().HaveCount(15);
        result[1].Split(' ').Should().HaveCount(2);
    }

    [Fact]
    public void SplitIntoSentences_FlushesTrailingWordsWithoutPunctuation()
    {
        const string text = "First sentence. Trailing words with no period";

        List<string> result = SentenceSplitter.SplitIntoSentences(text);

        result.Should().Equal("First sentence.", "Trailing words with no period");
    }

    [Fact]
    public void SplitIntoSentences_CollapsesExtraWhitespace()
    {
        const string text = "Too    many   spaces.";

        List<string> result = SentenceSplitter.SplitIntoSentences(text);

        result.Should().ContainSingle().Which.Should().Be("Too many spaces.");
    }
}
