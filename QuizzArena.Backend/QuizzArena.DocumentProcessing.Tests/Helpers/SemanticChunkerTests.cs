using FluentAssertions;
using QuizzArena.DocumentProcessing.Application.Helpers;

namespace QuizzArena.DocumentProcessing.Tests.Helpers;

public class SemanticChunkerTests
{
    private static readonly float[] _topicA = [1f, 0f];
    private static readonly float[] _topicB = [0f, 1f];

    private static string Words(int count) => string.Join(' ', Enumerable.Repeat("w", count));

    [Fact]
    public void Chunk_CountMismatch_Throws()
    {
        string[] sentences = ["a", "b"];
        float[][] embeddings = [_topicA];

        Action act = () => SemanticChunker.GenerateChunk(sentences, embeddings);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Chunk_NoSentences_ReturnsEmpty()
    {
        List<string> result = SemanticChunker.GenerateChunk([], []);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_SingleSentence_ReturnsThatSentenceAsOneChunk()
    {
        List<string> result = SemanticChunker.GenerateChunk(["only one"], [_topicA]);

        result.Should().ContainSingle().Which.Should().Be("only one");
    }

    [Fact]
    public void Chunk_TopicShift_ButChunkTooSmall_DoesNotSplit()
    {
        string[] sentences = ["short here", "different topic"];
        float[][] embeddings = [_topicA, _topicB];

        List<string> result = SemanticChunker.GenerateChunk(sentences, embeddings);

        result.Should().ContainSingle().Which.Should().Be("short here different topic");
    }

    [Fact]
    public void Chunk_TopicShift_WhenChunkLargeEnough_Splits()
    {
        string[] sentences = [Words(80), "different topic"];
        float[][] embeddings = [_topicA, _topicB];

        List<string> result = SemanticChunker.GenerateChunk(sentences, embeddings);

        result.Should().HaveCount(2);
        result[1].Should().Be("different topic");
    }

    [Fact]
    public void Chunk_SameTopic_ButExceedsMaxWords_ForcesSplit()
    {
        string[] sentences = [Words(250), Words(250)];
        float[][] embeddings = [_topicA, _topicA];

        List<string> result = SemanticChunker.GenerateChunk(sentences, embeddings);

        result.Should().HaveCount(2);
    }
}
