using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IDocumentChunkRepository
{
    Task<DocumentChunk> Create(Guid id);
}
