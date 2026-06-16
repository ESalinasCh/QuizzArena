using MassTransit;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using Shared.Messaging.Events;
using static QuizzArena.DocumentProcessing.Application.Ports.Out.IDocumentChunkRepository;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

public class TranscriptionRequestConsumer(
    IStorageServiceRepository storageServiceRepository,
    ITranscriptionService transcriptionService,
    IClassSourceRepository classSourceRepository
) : IConsumer<TranscriptionRequestCommand>
{
    public async Task Consume(ConsumeContext<TranscriptionRequestCommand> context)
    {
        try
        {
            TranscriptionRequestCommand command = context.Message;

            string transcribedText = await transcriptionService.TranscribeAudioAsync(command.FileUrl);

            string blobPath = $"class_{command.ClassSourceId}/transcription.txt";
            string transcriptUrl = await storageServiceRepository.UploadTextAsync(transcribedText, blobPath, "quiz-sources");

            ClassSource? classSource = await classSourceRepository.GetByIdAsync(command.ClassSourceId);
            if (classSource != null)
            {
                classSource.TranscriptUrl = transcriptUrl;
                classSource.Status = SourceStatus.Processing;
                await classSourceRepository.UpdateAsync(classSource);
            }

            await context.Publish<TranscriptionCompletedEvent>(new TranscriptionCompletedEvent
            {
                ClassSourceId = command.ClassSourceId,
                TranscriptUrl = transcriptUrl
            });
        }
        catch (HttpRequestException ex)
        {
            await context.Publish<TranscriptionFailedEvent>(new TranscriptionFailedEvent
            {
                ClassSourceId = context.Message.ClassSourceId,
                Reason = ex.Message
            });
        }
    }
}
