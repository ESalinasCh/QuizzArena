using MassTransit;
using Pgvector;
using QuizzArena.DocumentProcessing.Application.Helpers;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using Shared.Messaging.Events;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

/// <summary>
/// Indexing transcript to get most valuable chunks.
/// </summary>
/// <param name="storageServiceRepository"></param>
/// <param name="embeddingService"></param>
/// <param name="chunkClassifier"></param>
/// <param name="documentChunkRepository"></param>
public class IndexTranscriptConsumer(
    IStorageServiceRepository storageServiceRepository,
    IEmbeddingService embeddingService,
    IChunkClassifier chunkClassifier,
    IDocumentChunkRepository documentChunkRepository
) : IConsumer<IndexTranscriptCommand>
{
    // Keep only chunks the classifier marks academic with at least this confidence.
    private const double MinConfidence = 0.7;

    public async Task Consume(ConsumeContext<IndexTranscriptCommand> context)
    {
        IndexTranscriptCommand command = context.Message;

        try
        {
            string transcript = await storageServiceRepository.DownloadTextAsync(command.TranscriptUrl);

            List<string> sentences = SentenceSplitter.SplitIntoSentences(transcript);
            if (sentences.Count == 0)
            {
                await PublishCompleted(context, command.ClassSourceId, 0);
                return;
            }

            IReadOnlyList<float[]> sentenceEmbeddings = await embeddingService.EmbedInBatchesAsync(sentences);
            List<string> chunks = SemanticChunker.GenerateChunk(sentences, sentenceEmbeddings);

            // Classify each chunk and keep only the relevant ones. Category/confidence are used
            // here purely to filter — they are not persisted.
            List<string> keptChunks = [];
            foreach (string chunk in chunks)
            {
                ChunkClassification classification = await chunkClassifier.ClassifyAsync(chunk);
                if (classification.Category == ChunkCategory.Academic && classification.Confidence >= MinConfidence)
                {
                    keptChunks.Add(chunk);
                }
            }

            if (keptChunks.Count == 0)
            {
                await PublishCompleted(context, command.ClassSourceId, 0);
                return;
            }

            IReadOnlyList<float[]> chunkEmbeddings = await embeddingService.EmbedInBatchesAsync(keptChunks);

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

            await PublishCompleted(context, command.ClassSourceId, documentChunks.Count);
        }
        catch (Exception ex)
        {
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
}
