using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IDocumentChunkRepository
{
    public Task<IEnumerable<DocumentChunk>> GetChunksByClassSourceIdAsync(Guid classSourceId);
}

