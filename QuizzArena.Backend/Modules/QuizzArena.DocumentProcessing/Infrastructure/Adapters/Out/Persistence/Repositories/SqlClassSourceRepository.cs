
using System.Security.AccessControl;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Repositories;

public class SqlClassSourceRepository(DocumentProcessingDbContext context) : IClassSourceRepository
{
    public async Task<ClassSource> Create(ClassSource classSource)
    {
        context.ClassSource.Add(classSource);

        await context.SaveChangesAsync();

        return classSource;
    }
}
