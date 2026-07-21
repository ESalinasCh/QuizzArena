using MassTransit;
using Microsoft.Extensions.Logging;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using Shared.Messaging.Events;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

public partial class TranscriptionRequestConsumer(
    IStorageServiceRepository storageServiceRepository,
    ITranscriptionService transcriptionService,
    IClassSourceRepository classSourceRepository,
    ILogger<TranscriptionRequestConsumer> logger
) : IConsumer<TranscriptionRequestCommand>
{
    public async Task Consume(ConsumeContext<TranscriptionRequestCommand> context)
    {
        TranscriptionRequestCommand command = context.Message;

        try
        {
            LogStarted(logger, command.ClassSourceId, command.FileUrl);

            string transcribedText = await transcriptionService.TranscribeAudioAsync(command.FileUrl);
            LogTranscribed(logger, transcribedText.Length, command.ClassSourceId);

            string blobPath = $"class_{command.ClassSourceId}/transcription.txt";
            string transcriptUrl = await storageServiceRepository.UploadTextAsync(transcribedText, blobPath, "quiz-sources");
            LogStored(logger, transcriptUrl, command.ClassSourceId);

            ClassSource? classSource = await classSourceRepository.GetByIdAsync(command.ClassSourceId);
            if (classSource != null)
            {
                classSource.TranscriptUrl = transcriptUrl;
                classSource.Status = SourceStatus.Completed;
                await classSourceRepository.UpdateAsync(classSource);
            }
            else
            {
                LogClassSourceNotFound(logger, command.ClassSourceId);
            }

            await context.Publish<TranscriptionCompletedEvent>(new TranscriptionCompletedEvent
            {
                ClassSourceId = command.ClassSourceId,
                TranscriptUrl = transcriptUrl
            });
            LogCompletedPublished(logger, command.ClassSourceId);
        }
        catch (HttpRequestException ex)
        {
            LogFailed(logger, ex, command.ClassSourceId);

            await context.Publish<TranscriptionFailedEvent>(new TranscriptionFailedEvent
            {
                ClassSourceId = command.ClassSourceId,
                Reason = ex.Message
            });
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Transcription started for ClassSource {ClassSourceId} (file {FileUrl}).")]
    private static partial void LogStarted(ILogger logger, Guid classSourceId, string fileUrl);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transcription produced {CharCount} characters for ClassSource {ClassSourceId}.")]
    private static partial void LogTranscribed(ILogger logger, int charCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transcript stored at {TranscriptUrl} for ClassSource {ClassSourceId}.")]
    private static partial void LogStored(ILogger logger, string transcriptUrl, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "ClassSource {ClassSourceId} not found while saving transcript URL.")]
    private static partial void LogClassSourceNotFound(ILogger logger, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "TranscriptionCompletedEvent published for ClassSource {ClassSourceId}.")]
    private static partial void LogCompletedPublished(ILogger logger, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Transcription failed for ClassSource {ClassSourceId}.")]
    private static partial void LogFailed(ILogger logger, Exception exception, Guid classSourceId);
}
