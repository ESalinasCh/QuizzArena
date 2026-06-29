using MassTransit;
using Moq;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.DocumentProcessing.Tests.Consumers;

public class GenerationRequestConsumerTests
{
    private readonly Mock<IDocumentChunkRepository> _mockDocumentChunkRepository;
    private readonly Mock<IEmbeddingService> _mockEmbeddingService;
    private readonly Mock<ITextGenerationService> _mockTextGenerationService;
    private readonly Mock<ICosineSimilarity> _mockCosineSimilarity;
    private readonly Mock<IQuizContract> _mockQuizContract;
    private readonly Mock<IQuestionContract> _mockQuestionContract;
    private readonly Mock<ConsumeContext<GenerationRequestCommand>> _mockContext;

    private readonly GenerationRequestCommand _command;
    private readonly GenerationRequestConsumer _consumer;

    public GenerationRequestConsumerTests()
    {
        _mockDocumentChunkRepository = new Mock<IDocumentChunkRepository>();
        _mockEmbeddingService = new Mock<IEmbeddingService>();
        _mockTextGenerationService = new Mock<ITextGenerationService>();
        _mockCosineSimilarity = new Mock<ICosineSimilarity>();
        _mockQuizContract = new Mock<IQuizContract>();
        _mockQuestionContract = new Mock<IQuestionContract>();
        _mockContext = new Mock<ConsumeContext<GenerationRequestCommand>>();

        _command = new GenerationRequestCommand
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

        _consumer = new GenerationRequestConsumer(
            _mockDocumentChunkRepository.Object,
            _mockEmbeddingService.Object,
            _mockTextGenerationService.Object,
            _mockCosineSimilarity.Object,
            _mockQuizContract.Object,
            _mockQuestionContract.Object
        );
    }

    [Fact]
    public async Task Consume_NoDocumentChunks_ThrowsException()
    {
        // Arrange
        _mockDocumentChunkRepository
            .Setup(r => r.GetChunksByClassSourceIdAsync(_command.ClassSourceId))
            .ReturnsAsync(new List<DocumentChunk>());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_mockContext.Object));
        Assert.Equal("No document chunks found for the specified class source.", ex.Message);
    }

    [Fact]
    public async Task Consume_AllGeneratedQuestionsHaveInvalidCorrectAnswer_ThrowsException()
    {
        // Arrange
        SetupDocumentChunks();
        SetupGeneratedQuiz(new GenerationRequestConsumer.QuizGenerationFormat(
            "Invalid quiz",
            "Invalid answers",
            new List<GenerationRequestConsumer.QuestionGenerationFormat>
            {
                CreateGeneratedQuestion(correctAnswer: -1),
                CreateGeneratedQuestion(question: "Invalid second question?", correctAnswer: 2, options: new List<string> { "A", "B" }),
            }
        ));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_mockContext.Object));
        Assert.Equal("No valid questions found after filtering out questions with invalid CorrectAnswer indices.", ex.Message);
    }

    [Fact]
    public async Task Consume_JudgementEvaluationCountDoesNotMatchQuestions_ThrowsException()
    {
        // Arrange
        SetupDocumentChunks();
        SetupGeneratedQuiz(CreateValidQuiz());
        SetupJudgement(new GenerationRequestConsumer.QuizJudgementFormat(new List<GenerationRequestConsumer.QuestionJudgement>()));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_mockContext.Object));
        Assert.Equal("The AI judge returned an invalid or mismatched array of scores.", ex.Message);
    }

    [Fact]
    public async Task Consume_Success_CreatesQuestionsQuizAndPublishesFinalizeEvent()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var questionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        GenerationFinalizeProcessingRequestEvent? publishedEvent = null;
        QuizCreationRequestDTO? createdQuiz = null;
        List<QuestionCreationRequestDTO>? createdQuestions = null;

        SetupSuccessfulGeneration(questionIds, quizId);

        _mockQuestionContract
            .Setup(q => q.CreateQuestions(It.IsAny<List<QuestionCreationRequestDTO>>()))
            .Callback<List<QuestionCreationRequestDTO>>(questions => createdQuestions = questions)
            .ReturnsAsync(questionIds);

        _mockQuizContract
            .Setup(q => q.CreateQuiz(It.IsAny<QuizCreationRequestDTO>()))
            .Callback<QuizCreationRequestDTO>(quiz => createdQuiz = quiz)
            .ReturnsAsync(quizId);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<GenerationFinalizeProcessingRequestEvent>(), It.IsAny<CancellationToken>()))
            .Callback<GenerationFinalizeProcessingRequestEvent, CancellationToken>((message, _) => publishedEvent = message)
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
        SetupGeneratedQuiz(CreateValidQuiz());
        SetupJudgement(new GenerationRequestConsumer.QuizJudgementFormat(new List<GenerationRequestConsumer.QuestionJudgement>
        {
            new(0.9f, 0.9f, 0.9f),
            new(0.6f, 0.6f, 0.6f),
        }));
        SetupEmbeddings(new[] { new[] { 1f, 0f } });

        _mockQuestionContract
            .Setup(q => q.CreateQuestions(It.IsAny<List<QuestionCreationRequestDTO>>()))
            .Callback<List<QuestionCreationRequestDTO>>(questions => createdQuestions = questions)
            .ReturnsAsync(new List<Guid> { acceptedQuestionId });

        _mockQuizContract
            .Setup(q => q.CreateQuiz(It.IsAny<QuizCreationRequestDTO>()))
            .ReturnsAsync(quizId);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<GenerationFinalizeProcessingRequestEvent>(), It.IsAny<CancellationToken>()))
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
        SetupGeneratedQuiz(CreateValidQuiz());
        SetupJudgement(new GenerationRequestConsumer.QuizJudgementFormat(new List<GenerationRequestConsumer.QuestionJudgement>
        {
            new(0.9f, 0.9f, 0.9f),
            new(0.9f, 0.9f, 0.9f),
        }));
        SetupEmbeddings(new[]
        {
            new[] { 1f, 0f },
            new[] { 0.99f, 0.01f },
        });

        _mockCosineSimilarity
            .Setup(c => c.CalculateCosineSimilarity(It.IsAny<float[]>(), It.IsAny<float[]>()))
            .Returns(0.95d);

        _mockQuestionContract
            .Setup(q => q.CreateQuestions(It.IsAny<List<QuestionCreationRequestDTO>>()))
            .Callback<List<QuestionCreationRequestDTO>>(questions => createdQuestions = questions)
            .ReturnsAsync(new List<Guid> { acceptedQuestionId });

        _mockQuizContract
            .Setup(q => q.CreateQuiz(It.IsAny<QuizCreationRequestDTO>()))
            .ReturnsAsync(quizId);

        _mockContext
            .Setup(c => c.Publish(It.IsAny<GenerationFinalizeProcessingRequestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        Assert.NotNull(createdQuestions);
        Assert.Single(createdQuestions);
        Assert.Equal("What is dependency injection?", createdQuestions[0].Content);
    }

    private void SetupSuccessfulGeneration(List<Guid> questionIds, Guid quizId)
    {
        SetupDocumentChunks();
        SetupGeneratedQuiz(CreateValidQuiz());
        SetupJudgement(new GenerationRequestConsumer.QuizJudgementFormat(new List<GenerationRequestConsumer.QuestionJudgement>
        {
            new(0.9f, 0.9f, 0.9f),
            new(0.8f, 0.8f, 0.8f),
        }));
        SetupEmbeddings(new[]
        {
            new[] { 1f, 0f },
            new[] { 0f, 1f },
        });

        _mockCosineSimilarity
            .Setup(c => c.CalculateCosineSimilarity(It.IsAny<float[]>(), It.IsAny<float[]>()))
            .Returns(0.1d);

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

    private void SetupGeneratedQuiz(GenerationRequestConsumer.QuizGenerationFormat quiz)
    {
        _mockTextGenerationService
            .Setup(s => s.GenerateAsync<GenerationRequestConsumer.QuizGenerationFormat>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(quiz);
    }

    private void SetupJudgement(GenerationRequestConsumer.QuizJudgementFormat judgement)
    {
        _mockTextGenerationService
            .Setup(s => s.GenerateAsync<GenerationRequestConsumer.QuizJudgementFormat>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(judgement);
    }

    private void SetupEmbeddings(float[][] embeddings)
    {
        _mockEmbeddingService
            .Setup(s => s.GenerateMultipleEmbeddingsAsync(It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync(embeddings);
    }

    private static GenerationRequestConsumer.QuizGenerationFormat CreateValidQuiz()
    {
        return new GenerationRequestConsumer.QuizGenerationFormat(
            "Generated quiz",
            "Short quiz",
            new List<GenerationRequestConsumer.QuestionGenerationFormat>
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

    private static GenerationRequestConsumer.QuestionGenerationFormat CreateGeneratedQuestion(
        string question = "Question?",
        List<string>? options = null,
        int correctAnswer = 0,
        int valueScore = 1
    )
    {
        return new GenerationRequestConsumer.QuestionGenerationFormat(
            question,
            options ?? new List<string> { "A", "B" },
            correctAnswer,
            "Because the source says so.",
            valueScore
        );
    }
}
