namespace QuizzArena.DocumentProcessing.Domain.Entities;

public class DocumentProcessingJob
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid ProcessingJobId { get; set; }
}
