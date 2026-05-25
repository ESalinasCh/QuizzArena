using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Application.DTOs.ClassSource
{
    public class UploadClassSourceResponseDto
    {
        public SourceType SourceType;

        public SourceStatus Status = SourceStatus.Pending;
        public string Name { get; set; } = string.Empty;
        public string? TranscriptUrl { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool Deleted { get; set; } = false;
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
    }
}
