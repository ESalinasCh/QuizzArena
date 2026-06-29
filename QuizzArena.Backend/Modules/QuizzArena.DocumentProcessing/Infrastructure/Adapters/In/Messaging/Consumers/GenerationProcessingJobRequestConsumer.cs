using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

internal class GenerationProcessingJobRequestConsumer(
    IProcessingJobRepository processingJobRepository
) : IConsumer<GenerationProcessingJobRequestCommand>
{
    public async Task Consume(ConsumeContext<GenerationProcessingJobRequestCommand> context)
    {
        await processingJobRepository.CreateAsync(new ProcessingJob(){
            Id = context.Message.ProcessingJobId,
            Status = JobStatus.Processing,
            ErrorMessage = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DocumentProcessingJobs = new List<DocumentProcessingJob>() {
                new DocumentProcessingJob {
                    Id = context.Message.DocumentProcessingJobId,
                    DocumentId = context.Message.ClassSourceId,
                    ProcessingJobId = context.Message.ProcessingJobId
                }
            }
        });

        await context.Publish<GenerationRequestEvent>(new GenerationRequestEvent
        {
            ClassSourceId = context.Message.ClassSourceId,
            ProcessingJobId = context.Message.ProcessingJobId,
            DocumentProcessingJobId = context.Message.DocumentProcessingJobId,
            MinNumberOfOptions = context.Message.MinNumberOfOptions,
            MaxNumberOfOptions = context.Message.MaxNumberOfOptions,
            CreateMatch = context.Message.CreateMatch,
            BloomTaxonomy = context.Message.BloomTaxonomy
        });
    }
}
