using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Ingestion;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Ingestion;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

namespace QuizzArena.DocumentProcessing.Tests.Consumers;

public class TranscriptionRequestConsumerTests
{
    private const string TranscribedText = "Dependency injection decouples classes.";
    private const string TranscriptUrl = "https://storage.test/quiz-sources/transcription.txt";

    private readonly Mock<IStorageServiceRepository> _mockStorageServiceRepository;
    private readonly Mock<ITranscriptionService> _mockTranscriptionService;
    private readonly Mock<IClassSourceRepository> _mockClassSourceRepository;
    private readonly Mock<ConsumeContext<TranscriptionRequestCommand>> _mockContext;

    private readonly TranscriptionRequestCommand _command;
    private readonly ClassSource _classSource;
    private readonly TranscriptionRequestConsumer _consumer;

    public TranscriptionRequestConsumerTests()
    {
        _mockStorageServiceRepository = new Mock<IStorageServiceRepository>();
        _mockTranscriptionService = new Mock<ITranscriptionService>();
        _mockClassSourceRepository = new Mock<IClassSourceRepository>();
        _mockContext = new Mock<ConsumeContext<TranscriptionRequestCommand>>();

        _command = new TranscriptionRequestCommand
        {
            ClassSourceId = Guid.NewGuid(),
            FileUrl = "https://storage.test/quiz-sources/class-audio.mp3",
        };

        _classSource = new ClassSource
        {
            Id = _command.ClassSourceId,
            Status = SourceStatus.Processing,
            CourseId = Guid.NewGuid(),
        };

        _mockContext.Setup(c => c.Message).Returns(_command);

        _consumer = new TranscriptionRequestConsumer(
            _mockStorageServiceRepository.Object,
            _mockTranscriptionService.Object,
            _mockClassSourceRepository.Object,
            NullLogger<TranscriptionRequestConsumer>.Instance
        );
    }

    [Fact]
    public async Task Consume_ValidCommand_UpdatesClassSourceAndPublishesSuccessEvent()
    {
        // Arrange
        TranscriptionSuccessEvent? publishedEvent = null;
        SetupSuccessfulPipeline();

        _mockContext
            .Setup(c => c.Publish(It.IsAny<TranscriptionSuccessEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TranscriptionSuccessEvent, CancellationToken>((message, _) => publishedEvent = message)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.Equal(TranscriptUrl, _classSource.TranscriptUrl);
        Assert.Equal(SourceStatus.Completed, _classSource.Status);
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(_classSource), Times.Once);

        Assert.NotNull(publishedEvent);
        Assert.Equal(_command.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Equal(TranscriptUrl, publishedEvent.TranscriptUrl);
    }

    [Fact]
    public async Task Consume_ValidCommand_UploadsTranscriptToExpectedBlobPath()
    {
        // Arrange
        SetupSuccessfulPipeline();

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockTranscriptionService.Verify(s => s.TranscribeAudioAsync(_command.FileUrl), Times.Once);
        _mockStorageServiceRepository.Verify(
            s => s.UploadTextAsync(TranscribedText, $"class_{_command.ClassSourceId}/transcription.txt", "quiz-sources"),
            Times.Once
        );
    }

    [Fact]
    public async Task Consume_ClassSourceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync((ClassSource?)null);

        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _consumer.Consume(_mockContext.Object)
        );

        // Assert
        Assert.Contains(_command.ClassSourceId.ToString(), exception.Message, StringComparison.Ordinal);
        _mockTranscriptionService.Verify(s => s.TranscribeAudioAsync(It.IsAny<string>()), Times.Never);
        VerifyNoEventPublished();
    }

    [Fact]
    public async Task Consume_TranscriptionThrowsHttpRequestException_PublishesTranscriptionFailedEvent()
    {
        // Arrange
        TranscriptionFailedEvent? publishedEvent = null;
        SetupSuccessfulPipeline();

        _mockTranscriptionService
            .Setup(s => s.TranscribeAudioAsync(_command.FileUrl))
            .ThrowsAsync(new HttpRequestException("Transcription provider unavailable"));

        _mockContext
            .Setup(c => c.Publish(It.IsAny<TranscriptionFailedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TranscriptionFailedEvent, CancellationToken>((message, _) => publishedEvent = message)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Equal(_command.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Equal("Transcription provider unavailable", publishedEvent.ErrorMessage);

        _mockClassSourceRepository.Verify(r => r.UpdateAsync(It.IsAny<ClassSource>()), Times.Never);
        _mockContext.Verify(c => c.Publish(It.IsAny<TranscriptionSuccessEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_StorageThrowsHttpRequestException_PublishesFailedEventAndDoesNotUpdateClassSource()
    {
        // Arrange
        SetupSuccessfulPipeline();

        _mockStorageServiceRepository
            .Setup(s => s.UploadTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Blob storage unreachable"));

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.Equal(SourceStatus.Processing, _classSource.Status);
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(It.IsAny<ClassSource>()), Times.Never);
        _mockContext.Verify(c => c.Publish(It.IsAny<TranscriptionFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// The consumer only catches <see cref="HttpRequestException"/>, so any other failure escapes without
    /// publishing <see cref="TranscriptionFailedEvent"/> and the Ingestion saga stalls. See docs/consumer-defects.md.
    /// </summary>
    [Fact]
    public async Task Consume_StorageThrowsNonHttpException_PropagatesWithoutPublishingFailedEvent()
    {
        // Arrange
        SetupSuccessfulPipeline();

        _mockStorageServiceRepository
            .Setup(s => s.UploadTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new TimeoutException("Blob storage timed out"));

        // Act
        await Assert.ThrowsAsync<TimeoutException>(() => _consumer.Consume(_mockContext.Object));

        // Assert
        VerifyNoEventPublished();
    }

    private void SetupSuccessfulPipeline()
    {
        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync(_classSource);

        _mockTranscriptionService
            .Setup(s => s.TranscribeAudioAsync(_command.FileUrl))
            .ReturnsAsync(TranscribedText);

        _mockStorageServiceRepository
            .Setup(s => s.UploadTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(TranscriptUrl);

        _mockClassSourceRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ClassSource>()))
            .ReturnsAsync(_classSource);
    }

    private void VerifyNoEventPublished()
    {
        _mockContext.Verify(c => c.Publish(It.IsAny<TranscriptionSuccessEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockContext.Verify(c => c.Publish(It.IsAny<TranscriptionFailedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
