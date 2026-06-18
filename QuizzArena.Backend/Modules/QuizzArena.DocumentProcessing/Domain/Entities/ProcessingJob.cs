using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Domain.Entities;

public class ProcessingJob
{
    public Guid Id { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }

    public ICollection<DocumentProcessingJob> DocumentProcessingJobs { get; set; } = [];
}
