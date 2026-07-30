namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands.Compression;

public class CompressionStoringCommand
{
    public Guid ClassSourceId { get; set; }
    public string CompressedFileUrl { get; set; } = string.Empty;
}
