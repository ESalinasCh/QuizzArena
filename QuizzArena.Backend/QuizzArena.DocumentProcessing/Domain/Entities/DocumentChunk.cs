namespace QuizzArena.DocumentProcessing.Domain.Entities
{
    internal class DocumentChunk
    {
        public Guid Id { get; set; }
        public int ChunkOrder { get; set; }
        public string Content { get; set; } = string.Empty;

        public float[] Embedding { get; set; } = Array.Empty<float>();

        public Guid DocumentId { get; set; }
    }
}
