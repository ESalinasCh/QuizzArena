using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Domain.Entities;

internal class ProcessingJob
{
    public Guid Id { get; set; }
    public JobStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
}
