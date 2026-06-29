using Microsoft.EntityFrameworkCore;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Repositories;

public class SqlDocumentChunkRepository(DocumentProcessingDbContext context) : IDocumentChunkRepository
{
    public async Task<IEnumerable<DocumentChunk>> GetChunksByClassSourceIdAsync(Guid classSourceId)
    {
        IEnumerable<DocumentChunk> chunks = await context.DocumentChunk
            .Where(chunk => chunk.DocumentId == classSourceId)
            .ToListAsync();
        return chunks;
    }
}
