namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands;

public class GenerationRequestCommand
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; }
    public Guid DocumentProcessingJobId { get; set; }
}
