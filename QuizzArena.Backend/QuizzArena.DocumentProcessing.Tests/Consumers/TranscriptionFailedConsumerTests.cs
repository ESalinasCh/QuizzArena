using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Ingestion;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

namespace QuizzArena.DocumentProcessing.Tests.Consumers;

public class TranscriptionFailedConsumerTests
{
    private readonly Mock<IClassSourceRepository> _mockClassSourceRepository;
    private readonly Mock<ConsumeContext<TranscriptionFailedCommand>> _mockContext;

    private readonly TranscriptionFailedCommand _command;
    private readonly ClassSource _classSource;
    private readonly TranscriptionFailedConsumer _consumer;

    public TranscriptionFailedConsumerTests()
    {
        _mockClassSourceRepository = new Mock<IClassSourceRepository>();
        _mockContext = new Mock<ConsumeContext<TranscriptionFailedCommand>>();

        _command = new TranscriptionFailedCommand
        {
            ClassSourceId = Guid.NewGuid(),
            ErrorMessage = "Transcription provider unavailable",
        };

        _classSource = new ClassSource
        {
            Id = _command.ClassSourceId,
            Status = SourceStatus.Processing,
        };

        _mockContext.Setup(c => c.Message).Returns(_command);

        _consumer = new TranscriptionFailedConsumer(
            _mockClassSourceRepository.Object,
            NullLogger<TranscriptionFailedConsumer>.Instance
        );
    }

    [Fact]
    public async Task Consume_ExistingClassSource_SetsStatusToFailedAndUpdates()
    {
        // Arrange
        SetupClassSource(_classSource);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.Equal(SourceStatus.Failed, _classSource.Status);
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(_classSource), Times.Once);
    }

    [Fact]
    public async Task Consume_ClassSourceNotFound_DoesNotThrowAndDoesNotUpdate()
    {
        // Arrange
        SetupClassSource(null);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(It.IsAny<ClassSource>()), Times.Never);
    }

    [Fact]
    public async Task Consume_UpdateThrows_DoesNotPropagate()
    {
        // Arrange
        SetupClassSource(_classSource);
        _mockClassSourceRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ClassSource>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(_classSource), Times.Once);
    }

    [Fact]
    public async Task Consume_ExistingClassSource_NeverPublishesAnyEvent()
    {
        // Arrange
        SetupClassSource(_classSource);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.Message);
        _mockContext.VerifyNoOtherCalls();
    }

    private void SetupClassSource(ClassSource? classSource)
    {
        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync(classSource);

        _mockClassSourceRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ClassSource>()))
            .ReturnsAsync(_classSource);
    }
}
