using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Generation;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Generation;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Generation;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.DocumentProcessing.Tests.Consumers;

public class GenerationProcessingConsumerTests
{
    private static readonly float[][] _singleQuestionEmbedding = [[1f, 0f]];
    private static readonly float[][] _distinctQuestionEmbeddings = [[1f, 0f], [0f, 1f]];
    private static readonly float[][] _similarQuestionEmbeddings = [[1f, 0f], [0.99f, 0.01f]];

    private readonly Mock<IDocumentChunkRepository> _mockDocumentChunkRepository;
    private readonly Mock<IEmbeddingService> _mockEmbeddingService;
    private readonly Mock<ITextGenerationService> _mockTextGenerationService;
    private readonly Mock<IQuizContract> _mockQuizContract;
    private readonly Mock<IQuestionContract> _mockQuestionContract;
    private readonly Mock<ConsumeContext<GenerationProcessingCommand>> _mockContext;
    private readonly Mock<IOptions<QuizGenerationOptions>> _mockOptions;

    private readonly GenerationProcessingCommand _command;
    private readonly GenerationProcessingConsumer _consumer;
    private readonly QuizGenerationOptions _optionsValues;

    public GenerationProcessingConsumerTests()
    {
        _mockDocumentChunkRepository = new Mock<IDocumentChunkRepository>();
        _mockEmbeddingService = new Mock<IEmbeddingService>();
        _mockTextGenerationService = new Mock<ITextGenerationService>();
        _mockQuizContract = new Mock<IQuizContract>();
        _mockQuestionContract = new Mock<IQuestionContract>();
        _mockContext = new Mock<ConsumeContext<GenerationProcessingCommand>>();
        _mockOptions = new Mock<IOptions<QuizGenerationOptions>>();

        _optionsValues = new QuizGenerationOptions
        {
            CosineSimilarityThreshold = 0.92f,
            JudgementThreshold = 0.75f,
            QuestionEmbeddingModel = "bge-m3",
            QuizGenerationModel = "qwen2.5:7b-instruct",
            QuizJudgementModel = "llama3.1:8b-instruct-q4_K_M"
        };
        _mockOptions.Setup(o => o.Value).Returns(_optionsValues);

        _command = new GenerationProcessingCommand
        {
            ClassSourceId = Guid.NewGuid(),
            ProcessingJobId = Guid.NewGuid(),
            DocumentProcessingJobId = Guid.NewGuid(),
            NumberOfQuestions = 2,
            MinNumberOfOptions = 2,
            MaxNumberOfOptions = 4,
            CreateMatch = true,
        };

        _mockContext.Setup(c => c.Message).Returns(_command);

        _consumer = new GenerationProcessingConsumer(
            _mockDocumentChunkRepository.Object,
            _mockEmbeddingService.Object,
            _mockTextGenerationService.Object,
            _mockQuizContract.Object,
            _mockQuestionContract.Object,
            NullLogger<GenerationProcessingConsumer>.Instance,
            _mockOptions.Object
        );
    }

    [Fact]
    public async Task Consume_NoDocumentChunks_PublishesGenerationFailedEvent()
    {
        // Arrange
        _mockDocumentChunkRepository
            .Setup(r => r.GetChunksByClassSourceIdAsync(_command.ClassSourceId))
            .ReturnsAsync(new List<DocumentChunk>());

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.Publish(It.IsAny<GenerationFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_AllGeneratedQuestionsHaveInvalidCorrectAnswer_PublishesGenerationFailedEvent()
    {
        // Arrange
        SetupDocumentChunks();
        SetupGeneratedQuiz(new GenerationProcessingConsumer.QuizGenerationFormat(
            "Invalid quiz",
            "Invalid answers",
            new List<GenerationProcessingConsumer.QuestionGenerationFormat>
            {
            CreateGeneratedQuestion(correctAnswer: -1),
            CreateGeneratedQuestion(question: "Invalid second question?", correctAnswer: 2, options: new List<string> { "A", "B" }),
            }
        ));

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.Publish(It.IsAny<GenerationFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_NoSurvivingQuestions_PublishesGenerationFailedEvent()
    {
        // Arrange
        SetupDocumentChunks();
        SetupSequenceGeneration(CreateValidQuiz(), new GenerationProcessingConsumer.QuizJudgementFormat(new List<GenerationProcessingConsumer.QuestionJudgement>()));

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.Publish(It.IsAny<GenerationFailedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_Success_CreatesQuestionsQuizAndPublishesFinalizeEvent()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var questionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        GenerationEndingEvent? publishedEvent = null;
        QuizCreationRequestDTO? createdQuiz = null;
        List<QuestionCreationRequestDTO>? createdQuestions = null;

        SetupSuccessfulPipeline(questionIds, quizId);

        _mockQuestionContract
            .Setup(q => q.CreateQuestions(It.IsAny<List<QuestionCreationRequestDTO>>()))
            .Callback<List<QuestionCreationRequestDTO>>(questions => createdQuestions = questions)
            .ReturnsAsync(questionIds);

        _mockQuizContract
            .Setup(q => q.CreateQuiz(It.IsAny<QuizCreationRequestDTO>()))
            .Callback<QuizCreationRequestDTO>(quiz => createdQuiz = quiz)
            .ReturnsAsync(quizId);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<GenerationEndingEvent>(), It.IsAny<CancellationToken>()))
            .Callback<GenerationEndingEvent, CancellationToken>((message, _) => publishedEvent = message)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(createdQuestions);
        Assert.Equal(2, createdQuestions.Count);
        Assert.All(createdQuestions, question => Assert.Equal(_command.ProcessingJobId, question.ProcessingJobId));
        Assert.Equal("What is dependency injection?", createdQuestions[0].Content);
        Assert.Equal(0, createdQuestions[0].CorrectAnswer);

        Assert.NotNull(createdQuiz);
        Assert.Equal("Generated quiz", createdQuiz.Title);
        Assert.Equal("Short quiz", createdQuiz.Description);
        Assert.Equal(questionIds[0], createdQuiz.Questions[0].QuestionId);
        Assert.Equal(1, createdQuiz.Questions[0].Position);
        Assert.Equal(1, createdQuiz.Questions[0].ValueScore);
        Assert.Equal(questionIds[1], createdQuiz.Questions[1].QuestionId);
        Assert.Equal(2, createdQuiz.Questions[1].Position);
        Assert.Equal(2, createdQuiz.Questions[1].ValueScore);

        Assert.NotNull(publishedEvent);
        Assert.Equal(_command.ProcessingJobId, publishedEvent.ProcessingJobId);
        Assert.Equal(_command.ClassSourceId, publishedEvent.ClassSourceId);
        Assert.Equal(_command.DocumentProcessingJobId, publishedEvent.DocumentProcessingJobId);
        Assert.Equal(_command.CreateMatch, publishedEvent.CreateMatch);
        Assert.Equal("Generated quiz", publishedEvent.Title);
        Assert.Equal(2, publishedEvent.QuestionAmount);
        Assert.Equal(quizId, publishedEvent.QuizId);
    }

    [Fact]
    public async Task Consume_QuestionsBelowJudgementThreshold_AreNotCreated()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var acceptedQuestionId = Guid.NewGuid();
        List<QuestionCreationRequestDTO>? createdQuestions = null;

        SetupDocumentChunks();
        SetupSequenceGeneration(
            CreateValidQuiz(),
            new GenerationProcessingConsumer.QuizJudgementFormat(new List<GenerationProcessingConsumer.QuestionJudgement>
            {
                new(0.9f, 0.9f, 0.9f),
                new(0.6f, 0.6f, 0.6f),
            })
        );
        SetupEmbeddings(_singleQuestionEmbedding);

        _mockQuestionContract
            .Setup(q => q.CreateQuestions(It.IsAny<List<QuestionCreationRequestDTO>>()))
            .Callback<List<QuestionCreationRequestDTO>>(questions => createdQuestions = questions)
            .ReturnsAsync(new List<Guid> { acceptedQuestionId });

        _mockQuizContract
            .Setup(q => q.CreateQuiz(It.IsAny<QuizCreationRequestDTO>()))
            .ReturnsAsync(quizId);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<GenerationEndingEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(createdQuestions);
        Assert.Single(createdQuestions);
        Assert.Equal("What is dependency injection?", createdQuestions[0].Content);
    }

    [Fact]
    public async Task Consume_DuplicateQuestionsByCosineSimilarity_AreNotCreated()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var acceptedQuestionId = Guid.NewGuid();
        List<QuestionCreationRequestDTO>? createdQuestions = null;

        SetupDocumentChunks();
        SetupSequenceGeneration(
            CreateValidQuiz(),
            new GenerationProcessingConsumer.QuizJudgementFormat(new List<GenerationProcessingConsumer.QuestionJudgement>
            {
                new(0.9f, 0.9f, 0.9f),
                new(0.9f, 0.9f, 0.9f),
            })
        );
        SetupEmbeddings(_similarQuestionEmbeddings);

        _mockQuestionContract
            .Setup(q => q.CreateQuestions(It.IsAny<List<QuestionCreationRequestDTO>>()))
            .Callback<List<QuestionCreationRequestDTO>>(questions => createdQuestions = questions)
            .ReturnsAsync(new List<Guid> { acceptedQuestionId });

        _mockQuizContract
            .Setup(q => q.CreateQuiz(It.IsAny<QuizCreationRequestDTO>()))
            .ReturnsAsync(quizId);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<GenerationEndingEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(createdQuestions);
        Assert.Single(createdQuestions);
        Assert.Equal("What is dependency injection?", createdQuestions[0].Content);
    }

    private void SetupSuccessfulPipeline(List<Guid> questionIds, Guid quizId)
    {
        SetupDocumentChunks();
        SetupSequenceGeneration(
            CreateValidQuiz(),
            new GenerationProcessingConsumer.QuizJudgementFormat(new List<GenerationProcessingConsumer.QuestionJudgement>
            {
                new(0.9f, 0.9f, 0.9f),
                new(0.8f, 0.8f, 0.8f),
            })
        );
        SetupEmbeddings(_distinctQuestionEmbeddings);

        _mockQuestionContract
            .Setup(q => q.CreateQuestions(It.IsAny<List<QuestionCreationRequestDTO>>()))
            .ReturnsAsync(questionIds);

        _mockQuizContract
            .Setup(q => q.CreateQuiz(It.IsAny<QuizCreationRequestDTO>()))
            .ReturnsAsync(quizId);
    }

    private void SetupDocumentChunks()
    {
        _mockDocumentChunkRepository
            .Setup(r => r.GetChunksByClassSourceIdAsync(_command.ClassSourceId))
            .ReturnsAsync(new List<DocumentChunk>
            {
                new() { Id = Guid.NewGuid(), Content = "Dependency injection helps decouple classes.", ChunkOrder = 1 },
                new() { Id = Guid.NewGuid(), Content = "Unit tests verify behavior in isolation.", ChunkOrder = 2 },
            });
    }

    private void SetupGeneratedQuiz(GenerationProcessingConsumer.QuizGenerationFormat quiz)
    {
        _mockTextGenerationService
            .Setup(s => s.GenerateAsync<GenerationProcessingConsumer.QuizGenerationFormat>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(quiz);
    }

    private void SetupSequenceGeneration(GenerationProcessingConsumer.QuizGenerationFormat quiz, GenerationProcessingConsumer.QuizJudgementFormat judgement)
    {
        var sequence = new MockSequence();
        _mockTextGenerationService
            .InSequence(sequence)
            .Setup(s => s.GenerateAsync<GenerationProcessingConsumer.QuizGenerationFormat>(_optionsValues.QuizGenerationModel, It.IsAny<string>()))
            .ReturnsAsync(quiz);

        _mockTextGenerationService
            .InSequence(sequence)
            .Setup(s => s.GenerateAsync<GenerationProcessingConsumer.QuizJudgementFormat>(_optionsValues.QuizJudgementModel, It.IsAny<string>()))
            .ReturnsAsync(judgement);
    }

    private void SetupEmbeddings(float[][] embeddings)
    {
        _mockEmbeddingService
            .Setup(s => s.GenerateMultipleEmbeddingsAsync(It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync(embeddings);
    }

    private static GenerationProcessingConsumer.QuizGenerationFormat CreateValidQuiz()
    {
        return new GenerationProcessingConsumer.QuizGenerationFormat(
            "Generated quiz",
            "Short quiz",
            new List<GenerationProcessingConsumer.QuestionGenerationFormat>
            {
                CreateGeneratedQuestion(
                    question: "What is dependency injection?",
                    options: new List<string> { "A design technique", "A database" },
                    correctAnswer: 0,
                    valueScore: 1
                ),
                CreateGeneratedQuestion(
                    question: "What do unit tests verify?",
                    options: new List<string> { "Behavior", "Deployment" },
                    correctAnswer: 0,
                    valueScore: 2
                ),
            }
        );
    }

    private static GenerationProcessingConsumer.QuestionGenerationFormat CreateGeneratedQuestion(
        string question = "Question?",
        List<string>? options = null,
        int correctAnswer = 0,
        int valueScore = 1
    )
    {
        return new GenerationProcessingConsumer.QuestionGenerationFormat(
            question,
            options ?? new List<string> { "A", "B" },
            correctAnswer,
            "Because the source says so.",
            valueScore
        );
    }
}
