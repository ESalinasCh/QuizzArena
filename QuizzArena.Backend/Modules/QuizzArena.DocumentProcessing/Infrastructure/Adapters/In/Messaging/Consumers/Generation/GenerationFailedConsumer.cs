using MassTransit;
using Microsoft.Extensions.Logging;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Generation;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Generation;

internal partial class GenerationFailedConsumer(
    ILogger<TranscriptionFailedConsumer> logger,
    IProcessingJobRepository processingJobRepository
) : IConsumer<GenerationFailedCommand>
{
    public async Task Consume(ConsumeContext<GenerationFailedCommand> context)
    {

        GenerationFailedCommand command = context.Message;

        try
        {
            LogStarted(logger, command.ClassSourceId, command.ProcessingJobId, SourceStatus.Failed);
            ProcessingJob? processingJob = await processingJobRepository.GetByIdAsync(command.ProcessingJobId);

            if (processingJob == null)
            {
                LogProcessingJobNotFound(logger, command.ProcessingJobId);
                throw new InvalidOperationException($"Processing job with id {command.ProcessingJobId} not found.");
            }

            processingJob.Status = JobStatus.Failed;
            processingJob.ErrorMessage = command.ErrorMessage;
            await processingJobRepository.UpdateAsync(processingJob);
            LogUpdate(logger, command.ProcessingJobId, processingJob.Status);
        }
        catch (Exception ex)
        {
            LogUpdateFailed(logger, command.ProcessingJobId, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Generation failed for class source {classSourceId} with status {status}")]
    public static partial void LogStarted(ILogger logger, Guid classSourceId, Guid processingJobId, SourceStatus status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Processing job with id {processingJobId} not found.")]
    public static partial void LogProcessingJobNotFound(ILogger logger, Guid processingJobId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing job with id {processingJobId} updated with status {status}")]
    public static partial void LogUpdate(ILogger logger, Guid processingJobId, JobStatus status);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to update processing job with id {processingJobId}")]
    public static partial void LogUpdateFailed(ILogger logger, Guid processingJobId, Exception ex);
}
