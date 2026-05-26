using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;

namespace QuizzArena.DocumentProcessing.Application.Ports.In;

public interface IUploadSourceUseCase
{
    Task<UploadClassSourceResponseDto> Execute(UploadClassSourceRequestDto dto);
}
