namespace QuizzArena.DocumentProcessing.Domain.Entities
{
    internal class DocumentProcessingJob
    {
        public int Id { get; set; } 
        public Guid DocumentId { get; set; }
        public Guid ProcessingJobId { get; set; }
    }
}
