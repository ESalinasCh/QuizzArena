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
        public bool Deleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset DeletedAt { get; set; }

        // FK 
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
    }
}
