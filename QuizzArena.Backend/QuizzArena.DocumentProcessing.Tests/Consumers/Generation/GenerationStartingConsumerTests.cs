using MassTransit;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Generation;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Generation;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Generation;

namespace QuizzArena.DocumentProcessing.Tests.Consumers.Generation;

public class GenerationStartingConsumerTests
{
    private readonly Mock<IProcessingJobRepository> _mockProcessingJobRepository;
    private readonly Mock<ConsumeContext<GenerationStartingCommand>> _mockContext;

    private readonly GenerationStartingCommand _command;
    private readonly GenerationStartingConsumer _consumer;

    public GenerationStartingConsumerTests()
    {
        _mockProcessingJobRepository = new Mock<IProcessingJobRepository>();
        _mockContext = new Mock<ConsumeContext<GenerationStartingCommand>>();

        _command = new GenerationStartingCommand
        {
            ClassSourceId = Guid.NewGuid(),
            ProcessingJobId = Guid.NewGuid(),
            DocumentProcessingJobId = Guid.NewGuid(),
            NumberOfQuestions = 8,
            MinNumberOfOptions = 3,
            MaxNumberOfOptions = 5,
            CreateMatch = false,
            BloomTaxonomy = BloomTaxonomyLevel.Apply,
        };

        _mockContext.Setup(c => c.Message).Returns(_command);

        _consumer = new GenerationStartingConsumer(_mockProcessingJobRepository.Object);
    }

    [Fact]
    public async Task Consume_ValidCommand_CreatesProcessingJobWithProcessingStatus()
    {
        // Arrange
        ProcessingJob? createdJob = null;

        _mockProcessingJobRepository
            .Setup(r => r.CreateAsync(It.IsAny<ProcessingJob>()))
            .Callback<ProcessingJob>(job => createdJob = job)
            .ReturnsAsync((ProcessingJob job) => job);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(createdJob);
        Assert.Equal(_command.ProcessingJobId, createdJob.Id);
        Assert.Equal(JobStatus.Processing, createdJob.Status);
        Assert.Equal(string.Empty, createdJob.ErrorMessage);

        DocumentProcessingJob documentProcessingJob = Assert.Single(createdJob.DocumentProcessingJobs);
        Assert.Equal(_command.DocumentProcessingJobId, documentProcessingJob.Id);
        Assert.Equal(_command.ClassSourceId, documentProcessingJob.DocumentId);
        Assert.Equal(_command.ProcessingJobId, documentProcessingJob.ProcessingJobId);
    }

    [Fact]
    public async Task Consume_ValidCommand_PublishesGenerationProcessEventWithForwardedFields()
    {
        // Arrange
        GenerationProcessEvent? publishedEvent = null;

        SetupSuccessfulCreation();

        _mockContext
            .Setup(c => c.Publish(It.IsAny<GenerationProcessEvent>(), It.IsAny<CancellationToken>()))
            .Callback<GenerationProcessEvent, CancellationToken>((message, _) => publishedEvent = message)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Equal(_command.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Equal(_command.ProcessingJobId, publishedEvent.ProcessingJobId);
        Assert.Equal(_command.DocumentProcessingJobId, publishedEvent.DocumentProcessingJobId);
        Assert.Equal(_command.NumberOfQuestions, publishedEvent.NumberOfQuestions);
        Assert.Equal(_command.MinNumberOfOptions, publishedEvent.MinNumberOfOptions);
        Assert.Equal(_command.MaxNumberOfOptions, publishedEvent.MaxNumberOfOptions);
        Assert.Equal(_command.CreateMatch, publishedEvent.CreateMatch);
        Assert.Equal(_command.BloomTaxonomy, publishedEvent.BloomTaxonomy);
    }

    [Fact]
    public async Task Consume_RepositoryThrows_PropagatesAndDoesNotPublish()
    {
        // Arrange
        _mockProcessingJobRepository
            .Setup(r => r.CreateAsync(It.IsAny<ProcessingJob>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_mockContext.Object));

        // Assert
        _mockContext.Verify(c => c.Publish(It.IsAny<GenerationProcessEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupSuccessfulCreation()
    {
        _mockProcessingJobRepository
            .Setup(r => r.CreateAsync(It.IsAny<ProcessingJob>()))
            .ReturnsAsync((ProcessingJob job) => job);
    }
}
