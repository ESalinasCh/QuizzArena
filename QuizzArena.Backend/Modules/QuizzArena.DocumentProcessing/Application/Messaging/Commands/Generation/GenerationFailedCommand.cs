namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands.Generation;

public class GenerationFailedCommand
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; }
    public Guid DocumentProcessingJobId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
