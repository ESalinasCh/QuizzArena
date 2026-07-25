namespace QuizzArena.DocumentProcessing.Application.Messaging.Events;

public class TranscriptionFailedEvent
{
    public Guid ClassSourceId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
