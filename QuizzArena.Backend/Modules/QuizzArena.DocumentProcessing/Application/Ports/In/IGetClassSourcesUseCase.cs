using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using Shared.Contracts.DTOs;

namespace QuizzArena.DocumentProcessing.Application.Ports.In;

public interface IGetClassSourcesUseCase
{
    Task<List<GetClassSourceResponseDto>> Execute(Guid userId, PagedRequest query);
}
