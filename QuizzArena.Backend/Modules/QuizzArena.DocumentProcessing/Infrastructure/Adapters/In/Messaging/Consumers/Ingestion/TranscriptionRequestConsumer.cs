using MassTransit;
using Microsoft.Extensions.Logging;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Ingestion;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Ingestion;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;

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

            ClassSource? classSource = await classSourceRepository.GetByIdAsync(command.ClassSourceId);
            if (classSource == null)
            {
                LogClassSourceNotFound(logger, command.ClassSourceId);
                throw new InvalidOperationException($"Class source with ID {command.ClassSourceId} not found");
            }

            string transcribedText = await transcriptionService.TranscribeAudioAsync(command.FileUrl);
            LogTranscribed(logger, transcribedText.Length, command.ClassSourceId);

            string blobPath = $"class_{command.ClassSourceId}/transcription.txt";
            string transcriptUrl = await storageServiceRepository.UploadTextAsync(transcribedText, blobPath, "quiz-sources");
            LogStored(logger, transcriptUrl, command.ClassSourceId);

            classSource.TranscriptUrl = transcriptUrl;
            classSource.Status = SourceStatus.Completed;
            await classSourceRepository.UpdateAsync(classSource);

            await context.Publish(new TranscriptionSuccessEvent
            {
                ClassSourceId = command.ClassSourceId,
                TranscriptUrl = transcriptUrl
            });
            LogCompletedPublished(logger, command.ClassSourceId);
        }
        catch (HttpRequestException ex)
        {
            LogFailed(logger, ex, command.ClassSourceId, ex.Message);

            await context.Publish(new TranscriptionFailedEvent
            {
                ClassSourceId = command.ClassSourceId,
                ErrorMessage = ex.Message
            });
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Transcription started for ClassSource: {ClassSourceId} (file {FileUrl}).")]
    private static partial void LogStarted(ILogger logger, Guid classSourceId, string fileUrl);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Transcription for ClassSource: {ClassSourceId} produced {CharCount} characters.")]
    private static partial void LogTranscribed(ILogger logger, int charCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Transcription for ClassSource: {ClassSourceId} stored at {TranscriptUrl}.")]
    private static partial void LogStored(ILogger logger, string transcriptUrl, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CONSUMER] Transcription for ClassSource: {ClassSourceId} not found while saving transcript URL.")]
    private static partial void LogClassSourceNotFound(ILogger logger, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Transcription for ClassSource: {ClassSourceId} completed and TranscriptionCompletedEvent published.")]
    private static partial void LogCompletedPublished(ILogger logger, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CONSUMER] Transcription for ClassSource: {ClassSourceId} failed. Publishing TranscriptionFailedEvent with error: {ErrorMessage}.")]
    private static partial void LogFailed(ILogger logger, Exception exception, Guid classSourceId, string errorMessage);
}
