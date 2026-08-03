using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Indexing;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Indexing;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;

namespace QuizzArena.DocumentProcessing.Tests.Consumers.Indexing;

public class IndexingTranscriptConsumerTests
{
    /// <summary>
    /// Three sentences of 39, 40 and 35 characters. With a large MaxChunkSize they collapse into a single
    /// fragment; with MaxChunkSize = 40 the chunker emits one fragment per sentence.
    /// </summary>
    private const string Transcript =
        "Dependency injection decouples classes. Unit tests verify behavior in isolation. Integration tests cover the wiring.";

    private static readonly float[] _embedding = [0.1f, 0.2f];

    private static readonly List<string> _paragraphs =
    [
        "Dependency injection is a technique for decoupling collaborating classes.",
        "Unit tests assert behavior in isolation, while integration tests cover the wiring.",
    ];

    private readonly Mock<IStorageServiceRepository> _mockStorageServiceRepository;
    private readonly Mock<IEmbeddingService> _mockEmbeddingService;
    private readonly Mock<IDocumentChunkRepository> _mockDocumentChunkRepository;
    private readonly Mock<IClassSourceRepository> _mockClassSourceRepository;
    private readonly Mock<ITextGenerationService> _mockTextGenerationService;
    private readonly Mock<ConsumeContext<IndexingRequestCommand>> _mockContext;
    private readonly Mock<IOptions<IndexingOptions>> _mockOptions;

    private readonly IndexingRequestCommand _command;
    private readonly ClassSource _classSource;
    private readonly IndexingOptions _optionsValues;
    private readonly IndexingTranscriptConsumer _consumer;

    public IndexingTranscriptConsumerTests()
    {
        _mockStorageServiceRepository = new Mock<IStorageServiceRepository>();
        _mockEmbeddingService = new Mock<IEmbeddingService>();
        _mockDocumentChunkRepository = new Mock<IDocumentChunkRepository>();
        _mockClassSourceRepository = new Mock<IClassSourceRepository>();
        _mockTextGenerationService = new Mock<ITextGenerationService>();
        _mockContext = new Mock<ConsumeContext<IndexingRequestCommand>>();
        _mockOptions = new Mock<IOptions<IndexingOptions>>();

        _optionsValues = new IndexingOptions
        {
            EmbeddingModel = "bge-m3",
            IndexingModel = "qwen2.5:7b-instruct",
            MaxChunkSize = 10000,
        };
        _mockOptions.Setup(o => o.Value).Returns(_optionsValues);

        _command = new IndexingRequestCommand
        {
            ClassSourceId = Guid.NewGuid(),
            TranscriptUrl = "https://storage.test/quiz-sources/transcription.txt",
        };

        _classSource = new ClassSource
        {
            Id = _command.ClassSourceId,
            Status = SourceStatus.Processing,
        };

        _mockContext.Setup(c => c.Message).Returns(_command);

        _consumer = CreateConsumer();
    }

    [Fact]
    public async Task Consume_ValidTranscript_SavesChunksAndPublishesIndexingSuccessEvent()
    {
        // Arrange
        IReadOnlyList<DocumentChunk>? savedChunks = null;
        IndexingSuccessEvent? publishedEvent = null;

        SetupSuccessfulPipeline();

        _mockDocumentChunkRepository
            .Setup(r => r.SaveChunksAsync(It.IsAny<IReadOnlyList<DocumentChunk>>()))
            .Callback<IReadOnlyList<DocumentChunk>>(chunks => savedChunks = chunks)
            .Returns(Task.CompletedTask);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<IndexingSuccessEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IndexingSuccessEvent, CancellationToken>((message, _) => publishedEvent = message)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(savedChunks);
        Assert.Equal(_paragraphs.Count, savedChunks.Count);
        Assert.Equal(_paragraphs[0], savedChunks[0].Content);
        Assert.Equal(_paragraphs[1], savedChunks[1].Content);
        Assert.Equal(0, savedChunks[0].ChunkOrder);
        Assert.Equal(1, savedChunks[1].ChunkOrder);
        Assert.All(savedChunks, chunk => Assert.Equal(_command.ClassSourceId, chunk.DocumentId));
        Assert.All(savedChunks, chunk => Assert.NotNull(chunk.Embedding));
        Assert.All(savedChunks, chunk => Assert.NotEqual(Guid.Empty, chunk.Id));

        Assert.NotNull(publishedEvent);
        Assert.Equal(_command.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Equal(_paragraphs.Count, publishedEvent.StoredChunkCount);
    }

    [Fact]
    public async Task Consume_MultipleFragments_CallsTextGenerationOncePerFragment()
    {
        // Arrange
        SetupSuccessfulPipeline();
        _optionsValues.MaxChunkSize = 40;
        IndexingTranscriptConsumer consumer = CreateConsumer();

        // Act
        await consumer.Consume(_mockContext.Object);

        // Assert
        _mockTextGenerationService.Verify(
            s => s.GenerateAsync<IndexingTranscriptConsumer.Paragraphs>(_optionsValues.IndexingModel, It.IsAny<string>()),
            Times.Exactly(3)
        );
        _mockEmbeddingService.Verify(
            s => s.GenerateMultipleEmbeddingsAsync(_optionsValues.EmbeddingModel, It.IsAny<string[]>(), It.IsAny<int?>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Consume_ClassSourceNotFound_PublishesIndexingFailedEvent()
    {
        // Arrange
        SetupSuccessfulPipeline();
        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync((ClassSource?)null);

        // Act
        IndexingFailedEvent? publishedEvent = await ConsumeCapturingFailure();

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Equal(_command.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Contains(_command.ClassSourceId.ToString(), publishedEvent.ErrorMessage, StringComparison.Ordinal);
        _mockDocumentChunkRepository.Verify(r => r.SaveChunksAsync(It.IsAny<IReadOnlyList<DocumentChunk>>()), Times.Never);
    }

    [Fact]
    public async Task Consume_EmptyTranscript_PublishesIndexingFailedEvent()
    {
        // Arrange
        SetupSuccessfulPipeline();
        _mockStorageServiceRepository
            .Setup(s => s.DownloadTextAsync(_command.TranscriptUrl))
            .ReturnsAsync("   ");

        // Act
        IndexingFailedEvent? publishedEvent = await ConsumeCapturingFailure();

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Contains("is empty", publishedEvent.ErrorMessage, StringComparison.Ordinal);
        _mockTextGenerationService.Verify(
            s => s.GenerateAsync<IndexingTranscriptConsumer.Paragraphs>(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Consume_LlmReturnsNoParagraphs_PublishesIndexingFailedEvent()
    {
        // Arrange
        SetupSuccessfulPipeline();
        _mockTextGenerationService
            .Setup(s => s.GenerateAsync<IndexingTranscriptConsumer.Paragraphs>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new IndexingTranscriptConsumer.Paragraphs([]));

        // Act
        IndexingFailedEvent? publishedEvent = await ConsumeCapturingFailure();

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Contains("no valid document chunks", publishedEvent.ErrorMessage, StringComparison.Ordinal);
        _mockDocumentChunkRepository.Verify(r => r.SaveChunksAsync(It.IsAny<IReadOnlyList<DocumentChunk>>()), Times.Never);
    }

    [Fact]
    public async Task Consume_StorageDownloadThrows_PublishesIndexingFailedEvent()
    {
        // Arrange
        SetupSuccessfulPipeline();
        _mockStorageServiceRepository
            .Setup(s => s.DownloadTextAsync(_command.TranscriptUrl))
            .ThrowsAsync(new HttpRequestException("Blob storage unreachable"));

        // Act
        IndexingFailedEvent? publishedEvent = await ConsumeCapturingFailure();

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Equal("Blob storage unreachable", publishedEvent.ErrorMessage);
        _mockContext.Verify(c => c.Publish(It.IsAny<IndexingSuccessEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private IndexingTranscriptConsumer CreateConsumer() =>
        new(
            _mockStorageServiceRepository.Object,
            _mockEmbeddingService.Object,
            _mockDocumentChunkRepository.Object,
            _mockClassSourceRepository.Object,
            _mockTextGenerationService.Object,
            NullLogger<IndexingTranscriptConsumer>.Instance,
            _mockOptions.Object
        );

    private async Task<IndexingFailedEvent?> ConsumeCapturingFailure()
    {
        IndexingFailedEvent? publishedEvent = null;

        _mockContext
            .Setup(c => c.Publish(It.IsAny<IndexingFailedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IndexingFailedEvent, CancellationToken>((message, _) => publishedEvent = message)
            .Returns(Task.CompletedTask);

        await _consumer.Consume(_mockContext.Object);

        return publishedEvent;
    }

    private void SetupSuccessfulPipeline()
    {
        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync(_classSource);

        _mockStorageServiceRepository
            .Setup(s => s.DownloadTextAsync(_command.TranscriptUrl))
            .ReturnsAsync(Transcript);

        _mockTextGenerationService
            .Setup(s => s.GenerateAsync<IndexingTranscriptConsumer.Paragraphs>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new IndexingTranscriptConsumer.Paragraphs(_paragraphs));

        _mockEmbeddingService
            .Setup(s => s.GenerateMultipleEmbeddingsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<int?>()))
            .ReturnsAsync((string _, string[] prompts, int? _) => prompts.Select(_ => _embedding).ToArray());

        _mockDocumentChunkRepository
            .Setup(r => r.SaveChunksAsync(It.IsAny<IReadOnlyList<DocumentChunk>>()))
            .Returns(Task.CompletedTask);
    }
}
