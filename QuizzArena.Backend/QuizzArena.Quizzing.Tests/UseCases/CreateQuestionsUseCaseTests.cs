using AutoMapper;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.QuestionUseCases;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class CreateQuestionsUseCaseTests
{
    // Mocks
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IQuizQuestionRepository> _mockQuizQuestionRepository;
    private readonly Mock<IMapper> _mockMapper;

    // Real
    private readonly CreateQuestionDtoValidator _createQuestionDtoValidator;
    private readonly CreateQuestionsDtoValidator _createQuestionsDtoValidator;

    // Target unit test
    private readonly CreateQuestionsUseCase _createQuestionsUseCase;

    public CreateQuestionsUseCaseTests()
    {
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockQuizQuestionRepository = new Mock<IQuizQuestionRepository>();
        _mockMapper = new Mock<IMapper>();

        _createQuestionDtoValidator = new CreateQuestionDtoValidator();
        _createQuestionsDtoValidator = new CreateQuestionsDtoValidator(_createQuestionDtoValidator);

        _createQuestionsUseCase = new CreateQuestionsUseCase(
            _mockQuestionRepository.Object,
            _mockQuizQuestionRepository.Object,
            _mockMapper.Object,
            _createQuestionsDtoValidator
        );
    }

    [Fact]
    public async Task Execute_ValidOptions_CallsRepositoryCreateMultipleAsync()
    {
        // Arrange
        Guid classSourceId = Guid.NewGuid();
        Guid quizId = Guid.NewGuid();

        List<CreateQuestionDto> dtos =
        [
            new CreateQuestionDto
            {
                Content = "What is 2 + 2?",
                Type = QuestionType.MultipleChoice,
                Justification = "The sum of 2 and 2 is 4.",
                Position = 1,
                ValueScore = 10,
                Options = []
            },
            new CreateQuestionDto
            {
                Content = "What is the capital of France?",
                Type = QuestionType.MultipleChoice,
                Justification = "The capital of France is Paris.",
                Position = 2,
                ValueScore = 10,
                Options = []
            }
        ];

        List<Question> mappedQuestions =
        [
            new Question
            {
                Id = Guid.NewGuid(),
                Content = "What is 2 + 2?",
                Type = QuestionType.MultipleChoice,
                Justification = "The sum of 2 and 2 is 4.",
                ProcessingJobId = classSourceId,
                Status = QuestionStatus.Draft,
                Deleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new Question
            {
                Id = Guid.NewGuid(),
                Content = "What is the capital of France?",
                Type = QuestionType.MultipleChoice,
                Justification = "The capital of France is Paris.",
                ProcessingJobId = classSourceId,
                Status = QuestionStatus.Draft,
                Deleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        ];

        _mockMapper.Setup(m => m.Map<Question>(It.IsAny<CreateQuestionDto>()))
            .Returns((CreateQuestionDto dto) => mappedQuestions.First(q => q.Content == dto.Content));

        // Act
        await _createQuestionsUseCase.Execute(dtos, classSourceId, quizId);

        // Assert
        _mockMapper.Verify(m => m.Map<Question>(It.IsAny<CreateQuestionDto>()), Times.Exactly(dtos.Count));

        _mockQuestionRepository.Verify(
            r => r.CreateMultipleAsync(It.Is<List<Question>>(questions =>
                questions.Count == 2 &&
                questions.All(q => q.ProcessingJobId == classSourceId) &&
                questions.Any(q => q.Content == "What is 2 + 2?") &&
                questions.Any(q => q.Content == "What is the capital of France?")
            )),
            Times.Once);

        _mockQuizQuestionRepository.Verify(
            r => r.CreateMultipleAsync(It.Is<List<QuizQuestion>>(quizQuestions =>
                quizQuestions.Count == 2 &&
                quizQuestions.All(q => q.QuizId == quizId) &&
                quizQuestions.Any(q =>
                    q.QuestionId == mappedQuestions[0].Id &&
                    q.Position == dtos[0].Position &&
                    q.ValueScore == dtos[0].ValueScore) &&

                quizQuestions.Any(q =>
                    q.QuestionId == mappedQuestions[1].Id &&
                    q.Position == dtos[1].Position &&
                    q.ValueScore == dtos[1].ValueScore)
            )),
            Times.Once);
    }
}
