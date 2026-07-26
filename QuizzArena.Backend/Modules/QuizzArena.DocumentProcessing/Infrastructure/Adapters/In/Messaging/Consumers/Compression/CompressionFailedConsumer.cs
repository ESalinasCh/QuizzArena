using MassTransit;
using Microsoft.Extensions.Logging;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Compression;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Ingestion;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Compression;

internal partial class CompressionFailedConsumer(
    IClassSourceRepository classSourceRepository,
    ILogger<CompressionFailedConsumer> logger
) : IConsumer<CompressionFailedCommand>
{
    public async Task Consume(ConsumeContext<CompressionFailedCommand> context)
    {
        CompressionFailedCommand command = context.Message;

        try {
            LogStarted(logger, command.ClassSourceId, SourceStatus.Failed);

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

    [LoggerMessage(Level = LogLevel.Warning, Message = "[CONSUMER] Compression failed for ClassSource: {ClassSourceId}. Updating status to {Status}.")]
    private static partial void LogStarted(ILogger logger, Guid classSourceId, SourceStatus status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[CONSUMER] Compression failed for ClassSource: {ClassSourceId}. Status updated to {Status}.")]
    private static partial void LogUpdate(ILogger logger, Guid classSourceId, SourceStatus status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[CONSUMER] Compression failed for ClassSource: {ClassSourceId}. Class source not found.")]
    private static partial void LogClassSourceNotFound(ILogger logger, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CONSUMER] Compression failed for ClassSource: {ClassSourceId}. Failed to update status. Error: {ErrorMessage}")]
    private static partial void LogFailed(ILogger logger, Exception ex, Guid classSourceId, string errorMessage);
}
