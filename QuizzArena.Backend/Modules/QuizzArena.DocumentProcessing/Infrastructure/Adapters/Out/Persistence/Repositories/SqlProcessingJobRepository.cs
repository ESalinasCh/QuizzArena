using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlProcessingJobRepository(DocumentProcessingDbContext context) : IProcesingJobRepository
{
    public async Task<ProcessingJob> CreateAsync(ProcessingJob processingJob)
    {
        context.CourseStudents.Add(processingJob); // Change Course Students with ProcessingJob (Typo in DocumentProcessingDbContext)
        await context.SaveChangesAsync();
        return processingJob;
    }

    public async Task<ProcessingJob> UpdateAsync(ProcessingJob processingJob)
    {
        context.CourseStudents.Update(processingJob); // Change Course Students with ProcessingJob (Typo in DocumentProcessingDbContext)
        await context.SaveChangesAsync();
        return processingJob;
    }
}
