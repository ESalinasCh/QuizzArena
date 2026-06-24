namespace QuizzArena.DocumentProcessing.Application.Messaging.Events;

public class GenerationRequestEvent
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; } = Guid.NewGuid();
    public Guid DocumentProcessingJobId { get; set; } = Guid.NewGuid();
}
