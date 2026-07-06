using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pgvector;
using QuizzArena.DocumentProcessing.Application.Helpers;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

/// <summary>
/// Indexing transcript to get most valuable chunks.
/// </summary>
public partial class IndexTranscriptConsumer(
    IStorageServiceRepository storageServiceRepository,
    IEmbeddingService embeddingService,
    IChunkClassifier chunkClassifier,
    IDocumentChunkRepository documentChunkRepository,
    ILogger<IndexTranscriptConsumer> logger,
    IOptions<IndexingOptions> indexingConfig
) : IConsumer<IndexTranscriptCommand>
{
    private readonly IndexingOptions _indexingConfig = indexingConfig.Value;

    public async Task Consume(ConsumeContext<IndexTranscriptCommand> context)
    {
        IndexTranscriptCommand command = context.Message;

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

            IReadOnlyList<float[]> sentenceEmbeddings = await embeddingService.GenerateMultipleEmbeddingsAsync("bge-m3", sentences.ToArray());
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

            float[][] chunkEmbeddings = await embeddingService.GenerateMultipleEmbeddingsAsync("bge-m3", keptChunks.ToArray());

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
            LogFailed(logger, ex, command.ClassSourceId);

            await context.Publish(new IndexingFailedEvent
            {
                ClassSourceId = command.ClassSourceId,
                Reason = ex.Message,
            });
        }
    }

    private static Task PublishCompleted(ConsumeContext context, Guid classSourceId, int storedChunkCount) =>
        context.Publish(new IndexingCompletedEvent
        {
            ClassSourceId = classSourceId,
            StoredChunkCount = storedChunkCount,
        });

    [LoggerMessage(Level = LogLevel.Information, Message = "Indexing started for ClassSource {ClassSourceId} (transcript {TranscriptUrl}).")]
    private static partial void LogStarted(ILogger logger, Guid classSourceId, string transcriptUrl);

    [LoggerMessage(Level = LogLevel.Information, Message = "Split into {SentenceCount} sentences for ClassSource {ClassSourceId}.")]
    private static partial void LogSentences(ILogger logger, int sentenceCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Generated {ChunkCount} semantic chunks for ClassSource {ClassSourceId}.")]
    private static partial void LogChunks(ILogger logger, int chunkCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Kept {KeptCount} of {ChunkCount} chunks after classification for ClassSource {ClassSourceId}.")]
    private static partial void LogFiltered(ILogger logger, int keptCount, int chunkCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Stored {StoredCount} chunks for ClassSource {ClassSourceId}.")]
    private static partial void LogStored(ILogger logger, int storedCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Indexing failed for ClassSource {ClassSourceId}.")]
    private static partial void LogFailed(ILogger logger, Exception exception, Guid classSourceId);
}
