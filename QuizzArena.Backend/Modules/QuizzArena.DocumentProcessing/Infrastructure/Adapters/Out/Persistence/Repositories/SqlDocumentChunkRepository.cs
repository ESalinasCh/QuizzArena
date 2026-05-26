using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlDocumentChunkRepository : IDocumentChunkRepository
{
    public async Task<DocumentChunk> Create(Guid id)
    {
        return new DocumentChunk();
    }
}
