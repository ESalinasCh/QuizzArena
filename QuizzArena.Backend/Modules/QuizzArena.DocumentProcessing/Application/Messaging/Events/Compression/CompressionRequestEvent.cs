namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Compression;

public class CompressionRequestEvent
{
    public Guid ClassSourceId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}
