using MassTransit;
using Microsoft.Extensions.Hosting;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Infrastructure.Adapters.In.Messaging.Consumers;
using Shared.Messaging.Events;

namespace QuizzArena.Quizzing.Tests.Consumers;

public class GenerateQuizConsumerTests
{
    private readonly Mock<ICreateQuizUseCase> _mockCreateQuizUseCase;
    private readonly Mock<IHostEnvironment> _mockEnvironment;
    private readonly Mock<ConsumeContext<TranscriptionCompletedEvent>> _mockContext;

    private readonly TranscriptionCompletedEvent _message;
    private readonly GenerateQuizConsumer _consumer;

    public GenerateQuizConsumerTests()
    {
        _mockCreateQuizUseCase = new Mock<ICreateQuizUseCase>();
        _mockEnvironment = new Mock<IHostEnvironment>();
        _mockContext = new Mock<ConsumeContext<TranscriptionCompletedEvent>>();

        _message = new TranscriptionCompletedEvent
        {
            ClassSourceId = Guid.NewGuid(),
            TranscriptUrl = "https://storage.test/quiz-sources/transcription.txt",
        };

        _mockContext.Setup(c => c.Message).Returns(_message);

        _consumer = new GenerateQuizConsumer(_mockCreateQuizUseCase.Object, _mockEnvironment.Object);
    }

    [Fact]
    public async Task Consume_DevelopmentEnvironment_ExecutesCreateQuizUseCaseWithMessageClassSourceId()
    {
        // Arrange
        CreateQuizDto? executedQuiz = null;
        Guid executedClassSourceId = Guid.Empty;

        SetupEnvironment(Environments.Development);

        _mockCreateQuizUseCase
            .Setup(u => u.Execute(It.IsAny<CreateQuizDto>(), It.IsAny<Guid>()))
            .Callback<CreateQuizDto, Guid>((quiz, classSourceId) =>
            {
                executedQuiz = quiz;
                executedClassSourceId = classSourceId;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(executedQuiz);
        Assert.Equal("Math Quiz", executedQuiz.Title);
        Assert.NotEmpty(executedQuiz.Questions);
        Assert.Equal(_message.ClassSourceId, executedClassSourceId);
    }

    [Fact]
    public async Task Consume_DevelopmentEnvironment_PublishesQuizGenerationCompletedEvent()
    {
        // Arrange
        QuizGenerationCompletedEvent? publishedEvent = null;

        SetupEnvironment(Environments.Development);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<QuizGenerationCompletedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<QuizGenerationCompletedEvent, CancellationToken>((message, _) => publishedEvent = message)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Equal(_message.ClassSourceId, publishedEvent.ClassSourceId);
        _mockContext.Verify(c => c.Publish(It.IsAny<QuizGenerationFailedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_ProductionEnvironment_PublishesCompletedEventWithoutCallingUseCase()
    {
        // Arrange
        SetupEnvironment(Environments.Production);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockCreateQuizUseCase.Verify(u => u.Execute(It.IsAny<CreateQuizDto>(), It.IsAny<Guid>()), Times.Never);
        _mockContext.Verify(c => c.Publish(It.IsAny<QuizGenerationCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Outside Development and Production neither branch runs, yet the consumer still reports success.
    /// See docs/consumer-defects.md.
    /// </summary>
    [Fact]
    public async Task Consume_StagingEnvironment_PublishesCompletedEventWithoutCallingUseCase()
    {
        // Arrange
        SetupEnvironment(Environments.Staging);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockCreateQuizUseCase.Verify(u => u.Execute(It.IsAny<CreateQuizDto>(), It.IsAny<Guid>()), Times.Never);
        _mockContext.Verify(c => c.Publish(It.IsAny<QuizGenerationCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_UseCaseThrows_PublishesQuizGenerationFailedEventAndRethrows()
    {
        // Arrange
        QuizGenerationFailedEvent? publishedEvent = null;

        SetupEnvironment(Environments.Development);

        _mockCreateQuizUseCase
            .Setup(u => u.Execute(It.IsAny<CreateQuizDto>(), It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Quiz persistence failed"));

        _mockContext
            .Setup(c => c.Publish(It.IsAny<QuizGenerationFailedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<QuizGenerationFailedEvent, CancellationToken>((message, _) => publishedEvent = message)
            .Returns(Task.CompletedTask);

        // Act
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _consumer.Consume(_mockContext.Object)
        );

        // Assert
        Assert.Equal("Quiz persistence failed", exception.Message);
        Assert.NotNull(publishedEvent);
        Assert.Equal(_message.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Equal("Quiz persistence failed", publishedEvent.Reason);
        _mockContext.Verify(c => c.Publish(It.IsAny<QuizGenerationCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupEnvironment(string environmentName)
    {
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(environmentName);
    }
}
