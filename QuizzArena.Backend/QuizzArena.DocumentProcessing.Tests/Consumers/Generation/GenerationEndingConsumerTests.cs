using MassTransit;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Generation;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Generation;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.DocumentProcessing.Tests.Consumers.Generation;

public class GenerationEndingConsumerTests
{
    private readonly Mock<IProcessingJobRepository> _mockProcessingJobRepository;
    private readonly Mock<IClassSourceRepository> _mockClassSourceRepository;
    private readonly Mock<IMatchContract> _mockMatchContract;
    private readonly Mock<ConsumeContext<GenerationEndingCommand>> _mockContext;

    private readonly GenerationEndingCommand _command;
    private readonly ClassSource _classSource;
    private readonly ProcessingJob _processingJob;
    private readonly GenerationEndingConsumer _consumer;

    public GenerationEndingConsumerTests()
    {
        _mockProcessingJobRepository = new Mock<IProcessingJobRepository>();
        _mockClassSourceRepository = new Mock<IClassSourceRepository>();
        _mockMatchContract = new Mock<IMatchContract>();
        _mockContext = new Mock<ConsumeContext<GenerationEndingCommand>>();

        _command = new GenerationEndingCommand
        {
            ClassSourceId = Guid.NewGuid(),
            ProcessingJobId = Guid.NewGuid(),
            DocumentProcessingJobId = Guid.NewGuid(),
            CreateMatch = true,
            Title = "Generated quiz",
            QuestionAmount = 5,
            QuizId = Guid.NewGuid(),
        };

        _classSource = new ClassSource
        {
            Id = _command.ClassSourceId,
            CourseId = Guid.NewGuid(),
        };

        _processingJob = new ProcessingJob
        {
            Id = _command.ProcessingJobId,
            Status = JobStatus.Processing,
        };

        _mockContext.Setup(c => c.Message).Returns(_command);

        _consumer = new GenerationEndingConsumer(
            _mockProcessingJobRepository.Object,
            _mockClassSourceRepository.Object,
            _mockMatchContract.Object
        );
    }

    [Fact]
    public async Task Consume_CreateMatchTrue_CreatesAutomaticMatchWithClassSourceCourseId()
    {
        // Arrange
        MatchCreationAutomaticRequestDTO? createdMatch = null;

        SetupSuccessfulPipeline();

        _mockMatchContract
            .Setup(m => m.CreateAutomaticMatch(It.IsAny<MatchCreationAutomaticRequestDTO>()))
            .Callback<MatchCreationAutomaticRequestDTO>(match => createdMatch = match)
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(createdMatch);
        Assert.Equal(_command.Title, createdMatch.Title);
        Assert.Equal(_command.QuestionAmount, createdMatch.QuestionAmount);
        Assert.Equal(_command.QuizId, createdMatch.QuizId);
        Assert.Equal(_classSource.CourseId, createdMatch.CourseId);
    }

    [Fact]
    public async Task Consume_CreateMatchFalse_DoesNotCallMatchContractOrLoadClassSource()
    {
        // Arrange
        _command.CreateMatch = false;
        SetupSuccessfulPipeline();

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockClassSourceRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockMatchContract.Verify(m => m.CreateAutomaticMatch(It.IsAny<MatchCreationAutomaticRequestDTO>()), Times.Never);
        _mockProcessingJobRepository.Verify(r => r.UpdateAsync(_processingJob), Times.Once);
    }

    [Fact]
    public async Task Consume_CreateMatchTrueAndClassSourceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupSuccessfulPipeline();
        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync((ClassSource?)null);

        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _consumer.Consume(_mockContext.Object)
        );

        // Assert
        Assert.Equal("Invalid ClassSourceId", exception.Message);
        _mockMatchContract.Verify(m => m.CreateAutomaticMatch(It.IsAny<MatchCreationAutomaticRequestDTO>()), Times.Never);
        _mockProcessingJobRepository.Verify(r => r.UpdateAsync(It.IsAny<ProcessingJob>()), Times.Never);
    }

    [Fact]
    public async Task Consume_ProcessingJobNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupSuccessfulPipeline();
        _mockProcessingJobRepository
            .Setup(r => r.GetByIdAsync(_command.ProcessingJobId))
            .ReturnsAsync((ProcessingJob?)null);

        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _consumer.Consume(_mockContext.Object)
        );

        // Assert
        Assert.Equal("Invalid ProcessingJobId", exception.Message);
        _mockProcessingJobRepository.Verify(r => r.UpdateAsync(It.IsAny<ProcessingJob>()), Times.Never);
    }

    [Fact]
    public async Task Consume_ValidCommand_MarksProcessingJobCompleted()
    {
        // Arrange
        DateTimeOffset before = DateTimeOffset.UtcNow;
        SetupSuccessfulPipeline();

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.Equal(JobStatus.Completed, _processingJob.Status);
        Assert.NotNull(_processingJob.FinishedAt);
        Assert.InRange(_processingJob.FinishedAt.Value, before, DateTimeOffset.UtcNow);
        Assert.InRange(_processingJob.UpdatedAt, before, DateTimeOffset.UtcNow);
        _mockProcessingJobRepository.Verify(r => r.UpdateAsync(_processingJob), Times.Once);
    }

    /// <summary>
    /// GenerationSaga waits for a GenerationSuccessEvent while in the GenerationEnding state, but this consumer
    /// publishes nothing, so the saga never finalizes. See docs/consumer-defects.md.
    /// </summary>
    [Fact]
    public async Task Consume_ValidCommand_NeverPublishesAnyEvent()
    {
        // Arrange
        SetupSuccessfulPipeline();

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.Message);
        _mockContext.VerifyNoOtherCalls();
    }

    private void SetupSuccessfulPipeline()
    {
        _mockClassSourceRepository
            .Setup(r => r.GetByIdAsync(_command.ClassSourceId))
            .ReturnsAsync(_classSource);

        _mockMatchContract
            .Setup(m => m.CreateAutomaticMatch(It.IsAny<MatchCreationAutomaticRequestDTO>()))
            .ReturnsAsync(Guid.NewGuid());

        _mockProcessingJobRepository
            .Setup(r => r.GetByIdAsync(_command.ProcessingJobId))
            .ReturnsAsync(_processingJob);

        _mockProcessingJobRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ProcessingJob>()))
            .ReturnsAsync(_processingJob);
    }
}
