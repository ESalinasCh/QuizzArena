namespace QuizzArena.DocumentProcessing.Application.Messaging.Events;

public class GenerationCompletedEvent
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; }
    public Guid DocumentProcessingJobId { get; set; }

}
