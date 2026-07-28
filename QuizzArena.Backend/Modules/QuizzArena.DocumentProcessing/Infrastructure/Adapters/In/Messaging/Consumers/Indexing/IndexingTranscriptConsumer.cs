using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pgvector;
using QuizzArena.DocumentProcessing.Application.Helpers;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Indexing;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Indexing;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

/// <summary>
/// Indexing transcript to get most valuable chunks.
/// </summary>
public partial class IndexingTranscriptConsumer(
    IStorageServiceRepository storageServiceRepository,
    IEmbeddingService embeddingService,
    IDocumentChunkRepository documentChunkRepository,
    IClassSourceRepository classSourceRepository,
    ITextGenerationService textGenerationService,
    ILogger<IndexingTranscriptConsumer> logger,
    IOptions<IndexingOptions> indexingConfig
) : IConsumer<IndexingRequestCommand>
{
    private readonly IndexingOptions _indexingConfig = indexingConfig.Value;

    public record Paragraphs(List<string> Results);

    public static string GenerateIndexingPrompt(string transcriptFragment)
    {
        string prompt = $"""
        Analyze the following transcript excerpt and extract all technical, conceptual, and academic information relevant for an exam or quiz in a structured format.

        Instructions:
        1. Omit greetings, jokes, administrative announcements, or off-topic discussions.
        2. Condense the relevant information into well-written paragraphs grouped by topic or subtopic.
        3. Do not lose key concepts, formulas, or important explanations.
        4. Each summary or paragraph must be completely AUTONOMOUS and INDEPENDENT; it must be fully understandable on its own without relying on the context of other excerpts.

        Transcript:
        {transcriptFragment}
        """;

        return prompt;
    }

    public async Task Consume(ConsumeContext<IndexingRequestCommand> context)
    {
        IndexingRequestCommand command = context.Message;

        try
        {
            LogStarted(logger, command.ClassSourceId, command.TranscriptUrl);

            ClassSource? classSource = await classSourceRepository.GetByIdAsync(command.ClassSourceId);
            if (classSource == null)
            {
                LogClassSourceNotFound(logger, command.ClassSourceId);
                throw new InvalidOperationException($"Class source with ID {command.ClassSourceId} not found");
            }

            string transcript = await storageServiceRepository.DownloadTextAsync(command.TranscriptUrl);
            transcript = transcript.Trim();
            LogTranscript(logger, transcript.Length, command.ClassSourceId);
            if (transcript.Length == 0)
            {
                throw new InvalidOperationException($"Transcript for ClassSource {command.ClassSourceId} is empty");
            }

            List<string> sentences = SentenceSplitter.SplitIntoSentences(transcript, 20);
            LogSentences(logger, sentences.Count, command.ClassSourceId);
            if (sentences.Count == 0)
            {
                throw new InvalidOperationException($"Transcript for ClassSource {command.ClassSourceId} has no valid sentences");
            }

            List<string> fragments = TextChunker.ChunkList(sentences, _indexingConfig.MaxChunkSize);
            LogFragments(logger, _indexingConfig.MaxChunkSize, fragments.Count, command.ClassSourceId);
            if (fragments.Count == 0)
            {
                throw new InvalidOperationException($"Transcript for ClassSource {command.ClassSourceId} has no valid fragments");
            }

            List<string> rawChunks = new List<string>();
            foreach (string fragment in fragments)
            {
                Paragraphs docChunks = await textGenerationService.GenerateAsync<Paragraphs>(
                    _indexingConfig.IndexingModel,
                    GenerateIndexingPrompt(fragment)
                );
                rawChunks.AddRange(docChunks.Results);
            }

            LogDocumentChunks(logger, rawChunks.Count, command.ClassSourceId);
            if (rawChunks.Count == 0)
            {
                throw new InvalidOperationException($"Transcript for ClassSource {command.ClassSourceId} has no valid document chunks");
            }

            float[][] chunkEmbeddings = await embeddingService.GenerateMultipleEmbeddingsAsync(_indexingConfig.EmbeddingModel, rawChunks.ToArray());

            List<DocumentChunk> documentChunks = rawChunks
                .Select((content, index) => new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    ChunkOrder = index,
                    Content = content,
                    Embedding = new Vector(chunkEmbeddings[index]),
                    DocumentId = command.ClassSourceId,
                }).ToList();

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

    [LoggerMessage(Level = LogLevel.Error, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} not found..")]
    private static partial void LogClassSourceNotFound(ILogger logger, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} has transcript of length {TranscriptLength}.")]
    private static partial void LogTranscript(ILogger logger, int transcriptLength, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} split into {FragmentCount} fragments (max size: {MaxChunkSize}).")]
    private static partial void LogFragments(ILogger logger, int maxChunkSize, int fragmentCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} split into {SentenceCount} sentences.")]
    private static partial void LogSentences(ILogger logger, int sentenceCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} generated {ChunkCount} document chunks.")]
    private static partial void LogDocumentChunks(ILogger logger, int chunkCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} stored {StoredCount} chunks.")]
    private static partial void LogStored(ILogger logger, int storedCount, Guid classSourceId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CONSUMER] Indexing for ClassSource: {ClassSourceId} failed with error: {ErrorMessage}")]
    private static partial void LogFailed(ILogger logger, Exception exception, Guid classSourceId, string errorMessage);
}
