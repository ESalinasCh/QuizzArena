using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IClassSourceRepository
{
    public Task<ClassSource?> GetByIdAsync(Guid classSourceId);
    public Task<ClassSource> CreateAsync(ClassSource classSource);
    public Task<ClassSource> UpdateAsync(ClassSource classSource);
    public Task<List<(ClassSource Source, List<Guid> ProcessingJobsIds)>> GetByUserIdAsync(Guid userId);
}
