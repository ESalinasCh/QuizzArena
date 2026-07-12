using Microsoft.EntityFrameworkCore;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Repositories;

public class SqlClassSourceRepository(DocumentProcessingDbContext context) : IClassSourceRepository
{
    public async Task<ClassSource> CreateAsync(ClassSource classSource)
    {
        context.ClassSource.Add(classSource);
        await context.SaveChangesAsync();
        return classSource;
    }

    public async Task<ClassSource?> GetByIdAsync(Guid classSourceId)
    {
        return await context.ClassSource.FindAsync(classSourceId);
    }

    public async Task<ClassSource> UpdateAsync(ClassSource classSource)
    {
        context.ClassSource.Update(classSource);
        await context.SaveChangesAsync();
        return classSource;
    }

    public async Task<List<(ClassSource Source, List<Guid> ProcessingJobsIds)>> GetByUserIdAsync(Guid userId)
    {
        List<ClassSource> classSources = await context.ClassSource
            .AsNoTracking()
            .Where(cs => cs.UserId == userId && !cs.Deleted)
            .OrderByDescending(cs => cs.CreatedAt)
            .ToListAsync();

        var classSourceIds = classSources.Select(cs => cs.Id).ToList();

        Dictionary<Guid, List<Guid>> processingJobsByDocument = await context.DocumentProcessingJob
            .AsNoTracking()
            .Where(dpj => classSourceIds.Contains(dpj.DocumentId))
            .GroupBy(dpj => dpj.DocumentId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(dpj => dpj.ProcessingJobId).ToList());

        return classSources
            .Select(cs => (cs, processingJobsByDocument.GetValueOrDefault(cs.Id, [])))
            .ToList();
    }
}
