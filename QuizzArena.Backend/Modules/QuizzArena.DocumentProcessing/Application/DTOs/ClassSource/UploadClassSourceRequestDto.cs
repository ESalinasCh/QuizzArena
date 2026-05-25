namespace QuizzArena.DocumentProcessing.Application.DTOs.ClassSource
{
    public class UploadClassSourceRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
    }
}
