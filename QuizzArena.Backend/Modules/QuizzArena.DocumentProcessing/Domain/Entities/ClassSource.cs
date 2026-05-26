using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Domain.Entities;

internal class ClassSource
{
    public Guid Id { get; set; }
    public SourceType Type { get; set; }
    public SourceStatus Status { get; set; } = SourceStatus.Pending;
    public string Name { get; set; } = string.Empty;
    public string? TranscriptUrl { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }

    public ICollection<DocumentChunk> DocumentChunks { get; set; } = [];
}
