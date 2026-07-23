using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using Shared.Contracts.DTOs;

namespace QuizzArena.DocumentProcessing.Application.UseCases.ClassSources;

public class GetClassSourcesUseCase(IClassSourceRepository classSourceRepository) : IGetClassSourcesUseCase
{
    public async Task<List<GetClassSourceResponseDto>> Execute(Guid userId, PagedRequest query)
    {
        List<(ClassSource Source, List<Guid> ProcessingJobsIds)> results = await classSourceRepository.GetByUserIdAsync(userId, query);

        return results.Select(r => new GetClassSourceResponseDto
        {
            Id = r.Source.Id,
            Name = r.Source.Name,
            Status = r.Source.Status,
            CreatedAt = r.Source.CreatedAt,
            CourseId = r.Source.CourseId,
            ProcessingJobsIds = r.ProcessingJobsIds
        }).ToList();
    }
}
