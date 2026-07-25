using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pgvector;
using QuizzArena.DocumentProcessing.Application.Helpers;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Indexing;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Indexing;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

/// <summary>
/// Indexing transcript to get most valuable chunks.
/// </summary>
public partial class IndexingTranscriptConsumer(
    IStorageServiceRepository storageServiceRepository,
    IEmbeddingService embeddingService,
    IChunkClassifier chunkClassifier,
    IDocumentChunkRepository documentChunkRepository,
    ILogger<IndexingTranscriptConsumer> logger,
    IOptions<IndexingOptions> indexingConfig
) : IConsumer<IndexingRequestCommand>
{
    private readonly IndexingOptions _indexingConfig = indexingConfig.Value;

    public async Task Consume(ConsumeContext<IndexingRequestCommand> context)
    {
        IndexingRequestCommand command = context.Message;

        try
        {
            LogStarted(logger, command.ClassSourceId, command.TranscriptUrl);

            string transcript = await storageServiceRepository.DownloadTextAsync(command.TranscriptUrl);

            List<string> sentences = SentenceSplitter.SplitIntoSentences(transcript, 15);
            LogSentences(logger, sentences.Count, command.ClassSourceId);
            if (sentences.Count == 0)
            {
                await PublishCompleted(context, command.ClassSourceId, 0);
                return;
            }

            IReadOnlyList<float[]> sentenceEmbeddings = await embeddingService.GenerateMultipleEmbeddingsAsync(_indexingConfig.EmbeddingModel, sentences.ToArray());
            List<string> chunks = SemanticChunker.GenerateChunk(sentences, sentenceEmbeddings);
            LogChunks(logger, chunks.Count, command.ClassSourceId);

            List<string> keptChunks = [];
            foreach (string chunk in chunks)
            {
                ChunkClassification classification = await chunkClassifier.ClassifyAsync(chunk);
                if (classification.Category == ChunkCategory.Academic && classification.Confidence >= _indexingConfig.MinConfidence)
                {
                    keptChunks.Add(chunk);
                }
            }

            LogFiltered(logger, keptChunks.Count, chunks.Count, command.ClassSourceId);
            if (keptChunks.Count == 0)
            {
                await PublishCompleted(context, command.ClassSourceId, 0);
                return;
            }

            float[][] chunkEmbeddings = await embeddingService.GenerateMultipleEmbeddingsAsync(_indexingConfig.EmbeddingModel, keptChunks.ToArray());

            List<DocumentChunk> documentChunks = keptChunks
                .Select((content, index) => new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    ChunkOrder = index,
                    Content = content,
                    Embedding = new Vector(chunkEmbeddings[index]),
                    DocumentId = command.ClassSourceId,
                })
                .ToList();

            await documentChunkRepository.SaveChunksAsync(documentChunks);
            LogStored(logger, documentChunks.Count, command.ClassSourceId);

            await PublishCompleted(context, command.ClassSourceId, documentChunks.Count);
        }
        catch (Exception ex)
        {
            LogFailed(logger, ex, command.ClassSourceId, ex.Message);

            await context.Publish(new IndexingFailedEvent
            {
                ClassSourceId = command.ClassSourceId,
                ErrorMessage = ex.Message,
            });
        }
    }

    private static Task PublishCompleted(ConsumeContext context, Guid classSourceId, int storedChunkCount) =>
        context.Publish(new IndexingSuccessEvent
        {
            ClassSourceId = classSourceId,
            StoredChunkCount = storedChunkCount,
        });

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing started for ClassSource: {ClassSourceId} (transcript {TranscriptUrl}).")]
    private static partial void LogStarted(ILogger logger, Guid classSourceId, string transcriptUrl);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} split into {SentenceCount} sentences.")]
    private static partial void LogSentences(ILogger logger, int sentenceCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} generated {ChunkCount} semantic chunks.")]
    private static partial void LogChunks(ILogger logger, int chunkCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} kept {KeptCount} of {ChunkCount} chunks after classification.")]
    private static partial void LogFiltered(ILogger logger, int keptCount, int chunkCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} stored {StoredCount} chunks.")]
    private static partial void LogStored(ILogger logger, int storedCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} failed with error: {ErrorMessage}")]
    private static partial void LogFailed(ILogger logger, Exception exception, Guid classSourceId, string errorMessage);
}
