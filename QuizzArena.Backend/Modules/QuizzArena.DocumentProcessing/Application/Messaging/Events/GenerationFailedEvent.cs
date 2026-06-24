namespace QuizzArena.DocumentProcessing.Application.Messaging.Events;

public class GenerationFailedEvent
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; }
    public Guid DocumentProcessingJobId { get; set; }
}
