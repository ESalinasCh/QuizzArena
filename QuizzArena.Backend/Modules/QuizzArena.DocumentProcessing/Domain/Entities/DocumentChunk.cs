namespace QuizzArena.DocumentProcessing.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }
    public int ChunkOrder { get; set; }
    public string? Content { get; set; }
    public Pgvector.Vector? Embedding { get; set; } = null;

    public Guid DocumentId { get; set; }
}
