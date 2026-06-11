namespace Shared.Messaging.Events;

public class QuizGenerationFailedEvent
{
    public Guid ClassSourceId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
