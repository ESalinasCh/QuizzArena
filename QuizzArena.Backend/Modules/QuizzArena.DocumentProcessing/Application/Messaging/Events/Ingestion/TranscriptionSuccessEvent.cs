namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Ingestion;

public class TranscriptionSuccessEvent
{
    public Guid ClassSourceId { get; set; }
    public string TranscriptUrl { get; set; } = string.Empty;
}
