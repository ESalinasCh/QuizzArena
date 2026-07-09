using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;

public class GetClassSourceResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public SourceStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CourseId { get; set; }

    public List<Guid> ProcessingJobsIds { get; set; } = [];
}
