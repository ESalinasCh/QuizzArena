using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

internal interface IProcessingJobRepository
{
    public Task<ProcessingJob> CreateAsync(ProcessingJob processingJob);
    public Task<ProcessingJob> UpdateAsync(ProcessingJob processingJob);
}
