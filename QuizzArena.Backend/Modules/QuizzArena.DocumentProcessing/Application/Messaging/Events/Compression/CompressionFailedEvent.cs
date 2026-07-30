namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Compression;

public class CompressionFailedEvent
{
    public Guid ClassSourceId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
