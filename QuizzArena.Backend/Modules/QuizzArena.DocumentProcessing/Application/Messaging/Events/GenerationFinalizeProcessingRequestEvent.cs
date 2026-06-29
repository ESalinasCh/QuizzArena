namespace QuizzArena.DocumentProcessing.Application.Messaging.Events;

public class GenerationFinalizeProcessingRequestEvent
{
    public Guid ClassSourceId { get; set; }
    public Guid ProcessingJobId { get; set; } = Guid.NewGuid();
    public Guid DocumentProcessingJobId { get; set; } = Guid.NewGuid();
    public bool CreateMatch { get; set; } = true;
    public string Title { get; set; } = "Unnamed";
    public int QuestionAmount { get; set; }
    public Guid QuizId { get; set; }
}
