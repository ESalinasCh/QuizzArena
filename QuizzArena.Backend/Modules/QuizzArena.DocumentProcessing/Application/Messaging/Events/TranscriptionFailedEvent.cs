namespace QuizzArena.DocumentProcessing.Application.Messaging.Events;

public class TranscriptionFailedEvent
{
    public Guid ClassSourceId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
