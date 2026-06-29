namespace QuizzArena.DocumentProcessing.Application.Messaging.Commands;

public class GenerationTerminatingProcessingRequestCommand
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; }
    public Guid DocumentProcessingJobId { get; set; }
    public bool CreateMatch { get; set; } = true;
    public string Title { get; set; } = string.Empty;
    public int QuestionAmount { get; set; }
    public Guid QuizId { get; set; }
}
