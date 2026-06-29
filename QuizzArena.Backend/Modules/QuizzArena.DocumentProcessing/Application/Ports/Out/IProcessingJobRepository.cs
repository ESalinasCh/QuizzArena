using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IProcessingJobRepository
{
    public Task<ProcessingJob?> GetByIdAsync(Guid processingJobId);
    public Task<ProcessingJob> CreateAsync(ProcessingJob processingJob);
    public Task<ProcessingJob> UpdateAsync(ProcessingJob processingJob);
}
