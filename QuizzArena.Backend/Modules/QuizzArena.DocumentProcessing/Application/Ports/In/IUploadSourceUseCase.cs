using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.DTOs.DocumentChunk;

namespace QuizzArena.DocumentProcessing.Application.Ports.In
{
    public interface IUploadSourceUseCase
    {
        Task<UploadClassSourceResponseDto> Execute(UploadClassSourceRequestDto dto);
    }
}
