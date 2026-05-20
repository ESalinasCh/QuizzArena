using QuizzArena.DocumentProcessing.Application.DTOs.DocumentChunk;

namespace QuizzArena.DocumentProcessing.Application.Ports.In
{
    internal interface ICreateDocumentUseCase
    {
        Task<DocumentChunkDto> Execute(CreateDocumentChunkDto dto);
    }
}
