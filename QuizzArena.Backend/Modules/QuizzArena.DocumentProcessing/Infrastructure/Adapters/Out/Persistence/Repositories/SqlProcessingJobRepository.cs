using Microsoft.EntityFrameworkCore;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlProcessingJobRepository(DocumentProcessingDbContext context) : IProcessingJobRepository
{
    public async Task<ProcessingJob?> GetByIdAsync(Guid processingJobId)
    {
        return await context.ProcessingJobs.AsNoTracking().FirstOrDefaultAsync(p => p.Id == processingJobId);
    }

    public async Task<ProcessingJob> CreateAsync(ProcessingJob processingJob)
    {
        context.ProcessingJobs.Add(processingJob);
        await context.SaveChangesAsync();
        return processingJob;
    }

    public async Task<ProcessingJob> UpdateAsync(ProcessingJob processingJob)
    {
        context.ProcessingJobs.Update(processingJob);
        await context.SaveChangesAsync();
        return processingJob;
    }
}
