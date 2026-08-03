using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Generation;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Generation;

namespace QuizzArena.DocumentProcessing.Tests.Consumers.Generation;

public class GenerationFailedConsumerTests
{
    private readonly Mock<IProcessingJobRepository> _mockProcessingJobRepository;
    private readonly Mock<ConsumeContext<GenerationFailedCommand>> _mockContext;

    private readonly GenerationFailedCommand _command;
    private readonly ProcessingJob _processingJob;
    private readonly GenerationFailedConsumer _consumer;

    public GenerationFailedConsumerTests()
    {
        _mockProcessingJobRepository = new Mock<IProcessingJobRepository>();
        _mockContext = new Mock<ConsumeContext<GenerationFailedCommand>>();

        _command = new GenerationFailedCommand
        {
            ClassSourceId = Guid.NewGuid(),
            ProcessingJobId = Guid.NewGuid(),
            DocumentProcessingJobId = Guid.NewGuid(),
            ErrorMessage = "The model produced no usable questions",
        };

        _processingJob = new ProcessingJob
        {
            Id = _command.ProcessingJobId,
            Status = JobStatus.Processing,
        };

        _mockContext.Setup(c => c.Message).Returns(_command);

        // The consumer really does declare ILogger<TranscriptionFailedConsumer>; this is not a typo in the test.
        // See docs/consumer-defects.md.
        _consumer = new GenerationFailedConsumer(
            NullLogger<TranscriptionFailedConsumer>.Instance,
            _mockProcessingJobRepository.Object
        );
    }

    [Fact]
    public async Task Consume_ExistingProcessingJob_SetsFailedStatusAndCopiesErrorMessage()
    {
        // Arrange
        SetupProcessingJob(_processingJob);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.Equal(JobStatus.Failed, _processingJob.Status);
        Assert.Equal(_command.ErrorMessage, _processingJob.ErrorMessage);
        _mockProcessingJobRepository.Verify(r => r.UpdateAsync(_processingJob), Times.Once);
    }

    [Fact]
    public async Task Consume_ProcessingJobNotFound_DoesNotThrowAndDoesNotUpdate()
    {
        // Arrange
        SetupProcessingJob(null);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockProcessingJobRepository.Verify(r => r.UpdateAsync(It.IsAny<ProcessingJob>()), Times.Never);
    }

    [Fact]
    public async Task Consume_UpdateThrows_DoesNotPropagate()
    {
        // Arrange
        SetupProcessingJob(_processingJob);
        _mockProcessingJobRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ProcessingJob>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockProcessingJobRepository.Verify(r => r.UpdateAsync(_processingJob), Times.Once);
    }

    [Fact]
    public async Task Consume_ExistingProcessingJob_NeverPublishesAnyEvent()
    {
        // Arrange
        SetupProcessingJob(_processingJob);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.Message);
        _mockContext.VerifyNoOtherCalls();
    }

    private void SetupProcessingJob(ProcessingJob? processingJob)
    {
        _mockProcessingJobRepository
            .Setup(r => r.GetByIdAsync(_command.ProcessingJobId))
            .ReturnsAsync(processingJob);

        _mockProcessingJobRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ProcessingJob>()))
            .ReturnsAsync(_processingJob);
    }
}
