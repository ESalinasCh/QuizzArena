using Pgvector;

namespace QuizzArena.DocumentProcessing.Domain.Entities
{
    public class DocumentChunk
    {
        public Guid Id { get; set; }
        public int ChunkOrder { get; set; }
        public string Content { get; set; } = string.Empty;

        public Vector Embedding { get; set; } = null;

        public Guid DocumentId { get; set; }
    }
}
