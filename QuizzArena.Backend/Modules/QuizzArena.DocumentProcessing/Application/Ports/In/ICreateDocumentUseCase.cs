using QuizzArena.DocumentProcessing.Application.DTOs.DocumentChunk;

namespace QuizzArena.DocumentProcessing.Application.Ports.In;

public interface ICreateDocumentUseCase
{
    Task<DocumentChunkDto> Execute(CreateDocumentDto dto);
}
