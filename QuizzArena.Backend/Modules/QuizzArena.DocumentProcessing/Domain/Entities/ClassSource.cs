using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Domain.Entities
{
    internal class ClassSource
    {
        public Guid Id { get; set; } 
        public SourceType Type { get; set; }
        public SourceStatus Status { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TranscriptUrl { get; set; } = string.Empty;
        public DateTimeOffset UploadedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool Deleted { get; set; }

        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }

    }
}
