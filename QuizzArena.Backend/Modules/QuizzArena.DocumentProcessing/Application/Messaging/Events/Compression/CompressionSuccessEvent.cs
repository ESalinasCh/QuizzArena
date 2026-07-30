namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Compression;

public class CompressionSuccessEvent
{
    public Guid ClassSourceId { get; set; }
    public string CompressedFileUrl { get; set; } = string.Empty;
}
