using MassTransit;
using Microsoft.Extensions.Logging;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

internal partial class TranscriptionFailedConsumer(
    IClassSourceRepository classSourceRepository,
    ILogger<TranscriptionFailedConsumer> logger
) : IConsumer<TranscriptionFailedCommand>
{
    public async Task Consume(ConsumeContext<TranscriptionFailedCommand> context)
    {
        TranscriptionFailedCommand command = context.Message;
        LogStarted(logger, command.ClassSourceId, SourceStatus.Failed);

        try
        {
            ClassSource? classSource = await classSourceRepository.GetByIdAsync(command.ClassSourceId);
            if (classSource == null)
            {
                LogClassSourceNotFound(logger, command.ClassSourceId);
                throw new InvalidOperationException($"Class source with ID {command.ClassSourceId} not found");
            }

            classSource.Status = SourceStatus.Failed;
            await classSourceRepository.UpdateAsync(classSource);
            LogUpdate(logger, command.ClassSourceId, classSource.Status);
        }
        catch (Exception ex)
        {
            LogFailed(logger, ex, command.ClassSourceId, ex.Message);
        }

    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "[CONSUMER] Transcription failed for ClassSource: {ClassSourceId}. Updating status to {Status}.")]
    private static partial void LogStarted(ILogger logger, Guid classSourceId, SourceStatus status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[CONSUMER] Transcription failed for ClassSource: {ClassSourceId}. Status updated to {Status}.")]
    private static partial void LogUpdate(ILogger logger, Guid classSourceId, SourceStatus status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[CONSUMER] Transcription failed for ClassSource: {ClassSourceId}. Class source not found.")]
    private static partial void LogClassSourceNotFound(ILogger logger, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CONSUMER] Transcription failed for ClassSource: {ClassSourceId}. Failed to update status. Error: {ErrorMessage}")]
    private static partial void LogFailed(ILogger logger, Exception ex, Guid classSourceId, string errorMessage);
}
