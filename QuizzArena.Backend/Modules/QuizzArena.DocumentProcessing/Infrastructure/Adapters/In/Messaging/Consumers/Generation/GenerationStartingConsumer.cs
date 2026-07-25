using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Generation;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Generation;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Generation;

public class GenerationStartingConsumer(
    IProcessingJobRepository processingJobRepository
) : IConsumer<GenerationStartingCommand>
{
    public async Task Consume(ConsumeContext<GenerationStartingCommand> context)
    {
        await processingJobRepository.CreateAsync(new ProcessingJob()
        {
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

        await context.Publish(new GenerationProcessEvent
        {
            ClassSourceId = context.Message.ClassSourceId,
            ProcessingJobId = context.Message.ProcessingJobId,
            DocumentProcessingJobId = context.Message.DocumentProcessingJobId,
            NumberOfQuestions = context.Message.NumberOfQuestions,
            MinNumberOfOptions = context.Message.MinNumberOfOptions,
            MaxNumberOfOptions = context.Message.MaxNumberOfOptions,
            CreateMatch = context.Message.CreateMatch,
            BloomTaxonomy = context.Message.BloomTaxonomy
        });
    }
}
