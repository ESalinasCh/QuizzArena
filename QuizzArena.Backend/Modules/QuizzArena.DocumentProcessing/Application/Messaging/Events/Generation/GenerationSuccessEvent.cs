namespace QuizzArena.DocumentProcessing.Application.Messaging.Events.Generation;

public class GenerationSuccessEvent
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; }
    public Guid DocumentProcessingJobId { get; set; }
}
