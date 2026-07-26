using MassTransit;
using Microsoft.Extensions.Logging;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Compression;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Compression;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Compression;

public partial class CompressionStoringConsumer(
    IClassSourceRepository classSourceRepository,
    ILogger<CompressionStoringConsumer> logger
) : IConsumer<CompressionStoringCommand>
{
    public async Task Consume(ConsumeContext<CompressionStoringCommand> context)
    {
        CompressionStoringCommand command = context.Message;

        try {
            LogStarted(logger, command.ClassSourceId, command.CompressedFileUrl);

            ClassSource? classSource = await classSourceRepository.GetByIdAsync(command.ClassSourceId) ??
                throw new InvalidOperationException($"Class source with ID {command.ClassSourceId} not found");

            classSource.CompressedFileUrl = command.CompressedFileUrl;
            await classSourceRepository.UpdateAsync(classSource);

            await context.Publish(new CompressionSuccessEvent
            {
                ClassSourceId = command.ClassSourceId,
                CompressedFileUrl = command.CompressedFileUrl
            });

        }
        catch (Exception ex)
        {
            LogFailed(logger, ex, command.ClassSourceId, ex.Message);
            await context.Publish(new CompressionFailedEvent
            {
                ClassSourceId = command.ClassSourceId,
                ErrorMessage = ex.Message
            });
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Compression for ClassSource: {ClassSourceId}. Updating compressed file URL to {CompressedFileUrl}.")]
    private static partial void LogStarted(ILogger logger, Guid classSourceId, string compressedFileUrl);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CONSUMER] Compression failed for ClassSource: {ClassSourceId}. Failed to update status. Error: {ErrorMessage}")]
    private static partial void LogFailed(ILogger logger, Exception ex, Guid classSourceId, string errorMessage);
}
