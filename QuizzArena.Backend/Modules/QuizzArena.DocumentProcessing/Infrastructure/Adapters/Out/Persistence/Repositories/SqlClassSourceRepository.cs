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

    public async Task<ClassSource> UpdateAsync(ClassSource classSource){
        context.ClassSource.Update(classSource);
        await context.SaveChangesAsync();
        return classSource;
    }
}
