namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands.Compression;

public class CompressionFailedCommand
{
    public Guid ClassSourceId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
