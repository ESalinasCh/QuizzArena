using QuizzArena.DocumentProcessing.Application.DTOs.DocumentChunk;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Application.DTOs.ClassSource
{
    public class UploadSourceUseCase(
        IWhisperTranscriptionRepository WhisperRepository,
        IClassSourceRepository ClassSourceRepository
    ) : IUploadSourceUseCase
    {
        public async Task<UploadClassSourceResponseDto> Execute(UploadClassSourceRequestDto dto)
        {
            String transcript = await WhisperRepository.Transcribe();
            return new UploadClassSourceResponseDto();
        }
    }
}

// Port In -> UseCase
// Port Out -> Out Adapters
// In adapters (controlladores)