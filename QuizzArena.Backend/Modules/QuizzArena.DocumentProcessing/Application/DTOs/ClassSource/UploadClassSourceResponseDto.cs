using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;

public class UploadClassSourceResponseDto
{
    public Guid Id { get; set; }

    public SourceType SourceType { get; set; }

    public SourceStatus Status { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? TranscriptUrl { get; set; }

    public string? FileUrl { get; set; }

    public bool Deleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid CourseId { get; set; }

    public Guid UserId { get; set; }
}
