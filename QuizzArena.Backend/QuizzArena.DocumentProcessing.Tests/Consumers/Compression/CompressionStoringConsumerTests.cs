using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Compression;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Compression;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Compression;

namespace QuizzArena.DocumentProcessing.Tests.Consumers.Compression;

public class CompressionStoringConsumerTests
{
    private readonly Mock<IClassSourceRepository> _mockClassSourceRepository;
    private readonly Mock<ConsumeContext<CompressionStoringCommand>> _mockContext;

    private readonly CompressionStoringCommand _command;
    private readonly CompressionStoringConsumer _consumer;

    public CompressionStoringConsumerTests()
    {
        _mockClassSourceRepository = new Mock<IClassSourceRepository>();
        _mockContext = new Mock<ConsumeContext<CompressionStoringCommand>>();

        _command = new CompressionStoringCommand
        {
            ClassSourceId = Guid.NewGuid(),
            CompressedFileUrl = "https://storage.example.com/compressed/file.zip"
        };

        _mockContext.Setup(c => c.Message).Returns(_command);

        _consumer = new CompressionStoringConsumer(
            _mockClassSourceRepository.Object,
            NullLogger<CompressionStoringConsumer>.Instance
        );
    }

    [Fact]
    public async Task Consume_Success_UpdatesClassSourceAndPublishesCompressionSuccessEvent()
    {
        // Arrange
        var classSource = new ClassSource
        {
            Id = _command.ClassSourceId,
            CompressedFileUrl = null
        };

        CompressionSuccessEvent? publishedEvent = null;

        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync(classSource);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<CompressionSuccessEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CompressionSuccessEvent, CancellationToken>((evt, _) => publishedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.Equal(_command.CompressedFileUrl, classSource.CompressedFileUrl);
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(It.Is<ClassSource>(cs =>
            cs.Id == _command.ClassSourceId &&
            cs.CompressedFileUrl == _command.CompressedFileUrl)), Times.Once);

        Assert.NotNull(publishedEvent);
        Assert.Equal(_command.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Equal(_command.CompressedFileUrl, publishedEvent.CompressedFileUrl);
    }

    [Fact]
    public async Task Consume_ClassSourceNotFound_PublishesCompressionFailedEvent()
    {
        // Arrange
        CompressionFailedEvent? publishedEvent = null;

        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync((ClassSource?)null);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<CompressionFailedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CompressionFailedEvent, CancellationToken>((evt, _) => publishedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(It.IsAny<ClassSource>()), Times.Never);
        _mockContext.Verify(c => c.Publish(It.IsAny<CompressionSuccessEvent>(), It.IsAny<CancellationToken>()), Times.Never);

        Assert.NotNull(publishedEvent);
        Assert.Equal(_command.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Contains(_command.ClassSourceId.ToString(), publishedEvent.ErrorMessage);
    }

    [Fact]
    public async Task Consume_RepositoryUpdateThrowsException_PublishesCompressionFailedEvent()
    {
        // Arrange
        var classSource = new ClassSource { Id = _command.ClassSourceId };
        const string errorMessage = "Database update error";
        CompressionFailedEvent? publishedEvent = null;

        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync(classSource);

        _mockClassSourceRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ClassSource>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        _mockContext
            .Setup(c => c.Publish(It.IsAny<CompressionFailedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CompressionFailedEvent, CancellationToken>((evt, _) => publishedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(It.IsAny<ClassSource>()), Times.Once);
        _mockContext.Verify(c => c.Publish(It.IsAny<CompressionSuccessEvent>(), It.IsAny<CancellationToken>()), Times.Never);

        Assert.NotNull(publishedEvent);
        Assert.Equal(_command.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Equal(errorMessage, publishedEvent.ErrorMessage);
    }
}
