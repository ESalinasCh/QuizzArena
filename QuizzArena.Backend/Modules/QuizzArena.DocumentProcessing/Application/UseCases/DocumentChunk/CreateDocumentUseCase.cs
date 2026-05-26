using QuizzArena.DocumentProcessing.Application.DTOs.DocumentChunk;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Application.UseCases.DocumentChunk;

public class CreateDocumentUseCase(IDocumentChunkRepository repository) : ICreateDocumentUseCase
{
    public async Task<DocumentChunkDto> Execute(CreateDocumentDto dto)
    {
        return new DocumentChunkDto();
    }
}
