using FluentAssertions;
using QuizzArena.DocumentProcessing.Application.Helpers;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Tests.Helpers;

public class SourceTypeResolverTests
{
    [Theory]
    [InlineData("clase.mp3", SourceType.Audio)]
    [InlineData("clase.wav", SourceType.Audio)]
    [InlineData("clase.mp4", SourceType.Video)]
    [InlineData("clase.txt", SourceType.Text)]
    public void Resolve_WithSupportedExtension_ReturnsMatchingSourceType(string fileName, SourceType expected)
    {
        SourceTypeResolver.Resolve(fileName).Should().Be(expected);
    }

    [Theory]
    [InlineData("CLASE.MP4", SourceType.Video)]
    [InlineData("Clase.Mp3", SourceType.Audio)]
    [InlineData("clase.TXT", SourceType.Text)]
    public void Resolve_IsCaseInsensitive(string fileName, SourceType expected)
    {
        SourceTypeResolver.Resolve(fileName).Should().Be(expected);
    }

    [Fact]
    public void Resolve_WithMultipleDots_UsesLastExtension()
    {
        SourceTypeResolver.Resolve("clase.final.v2.mp4").Should().Be(SourceType.Video);
    }

    [Theory]
    [InlineData("clase.exe")]
    [InlineData("clase.pdf")]
    [InlineData("clase")]
    [InlineData("")]
    public void Resolve_WithUnsupportedExtension_ThrowsArgumentException(string fileName)
    {
        Action act = () => SourceTypeResolver.Resolve(fileName);

        act.Should().Throw<ArgumentException>().WithMessage("Unsupported file type");
    }
}
