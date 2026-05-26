using QuizzArena.DocumentProcessing.Application.DTOs.DocumentChunk;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Application.DTOs.ClassSource
{
    public class UploadSourceUseCase : IUploadSourceUseCase
    {
        public async Task<UploadClassSourceResponseDto> Execute(UploadClassSourceRequestDto dto)
        {
            return new UploadClassSourceResponseDto();
        }
    }
}

// Port In -> UseCase
// Port Out -> Out Adapters
// In adapters (controlladores)