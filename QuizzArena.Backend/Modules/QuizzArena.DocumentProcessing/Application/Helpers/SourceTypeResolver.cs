using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Application.Helpers;

internal static class SourceTypeResolver
{
    internal static SourceType Resolve(string fileName)
    {
        string extension =
            Path.GetExtension(fileName);

        return extension.ToLowerInvariant() switch
        {
            ".mp3" => SourceType.Audio,
            ".wav" => SourceType.Audio,
            ".mp4" => SourceType.Video,

            _ => throw new ArgumentException(
                "Unsupported file type"
            )
        };
    }
}
