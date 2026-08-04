using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Compression;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Compression;

namespace QuizzArena.DocumentProcessing.Tests.Consumers.Compression;

public class CompressionFailedConsumerTests
{
    private readonly Mock<IClassSourceRepository> _mockClassSourceRepository;
    private readonly Mock<ConsumeContext<CompressionFailedCommand>> _mockContext;

    private readonly CompressionFailedCommand _command;
    private readonly CompressionFailedConsumer _consumer;

    public CompressionFailedConsumerTests()
    {
        _mockClassSourceRepository = new Mock<IClassSourceRepository>();
        _mockContext = new Mock<ConsumeContext<CompressionFailedCommand>>();

        _command = new CompressionFailedCommand
        {
            ClassSourceId = Guid.NewGuid()
        };

        _mockContext.Setup(c => c.Message).Returns(_command);

        _consumer = new CompressionFailedConsumer(
            _mockClassSourceRepository.Object,
            NullLogger<CompressionFailedConsumer>.Instance
        );
    }

    [Fact]
    public async Task Consume_ClassSourceNotFound_DoesNotUpdateStatusAndHandlesException()
    {
        // Arrange
        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync((ClassSource?)null);

        // Act
        var exception = await Record.ExceptionAsync(() => _consumer.Consume(_mockContext.Object));

        // Assert
        Assert.Null(exception);
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(It.IsAny<ClassSource>()), Times.Never);
    }

    [Fact]
    public async Task Consume_ClassSourceExists_UpdatesStatusToFailed()
    {
        // Arrange
        var classSource = new ClassSource
        {
            Id = _command.ClassSourceId,
            Status = SourceStatus.Pending
        };

        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync(classSource);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.Equal(SourceStatus.Failed, classSource.Status);
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(It.Is<ClassSource>(cs => cs.Id == _command.ClassSourceId && cs.Status == SourceStatus.Failed)), Times.Once);
    }

    [Fact]
    public async Task Consume_RepositoryUpdateThrowsException_LogsAndHandlesException()
    {
        // Arrange
        var classSource = new ClassSource
        {
            Id = _command.ClassSourceId,
            Status = SourceStatus.Pending
        };

        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync(classSource);

        _mockClassSourceRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ClassSource>()))
            .ThrowsAsync(new InvalidOperationException("Database connection error"));

        // Act
        var exception = await Record.ExceptionAsync(() => _consumer.Consume(_mockContext.Object));

        // Assert
        Assert.Null(exception);
        _mockClassSourceRepository.Verify(r => r.UpdateAsync(It.IsAny<ClassSource>()), Times.Once);
    }
}
