namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Ingestion;

public class TranscriptionRequestEvent
{
    public Guid ClassSourceId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}
