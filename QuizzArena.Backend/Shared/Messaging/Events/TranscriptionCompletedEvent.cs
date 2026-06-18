namespace Shared.Messaging.Events;

public class TranscriptionCompletedEvent
{
    public Guid ClassSourceId { get; set; }
    public string TranscriptUrl { get; set; } = string.Empty;
}
