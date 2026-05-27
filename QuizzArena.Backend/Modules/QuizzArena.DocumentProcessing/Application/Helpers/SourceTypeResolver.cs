using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Application.Services;

public static class SourceTypeResolver
{
    public static SourceType Resolve(string fileName)
    {
        string extension =
            Path.GetExtension(fileName);

        return extension.ToLower() switch
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
