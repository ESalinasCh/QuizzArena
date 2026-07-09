using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;

namespace QuizzArena.DocumentProcessing.Application.Ports.In;

public interface IGetClassSourcesUseCase
{
    Task<List<GetClassSourceResponseDto>> Execute(Guid userId);
}
