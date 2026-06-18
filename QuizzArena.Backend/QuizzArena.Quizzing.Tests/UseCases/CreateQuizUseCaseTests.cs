using AutoMapper;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.QuizUseCases;
using QuizzArena.Quizzing.Application.Validators.Quiz;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class CreateQuizUseCaseTests
{
    // Mocks
    private readonly Mock<IQuizRepository> _mockQuizRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ICreateQuestionsUseCase> _mockCreateQuestionsUseCase;
    private readonly Mock<ICreateOptionsUseCase> _mockCreateOptionsUseCase;

    // Real
    private readonly CreateQuizDtoValidator _createQuizDtoValidator;

    // Target unit test
    private readonly CreateQuizUseCase _createQuizUseCase;

    public CreateQuizUseCaseTests()
    {
        _mockQuizRepository = new Mock<IQuizRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockCreateQuestionsUseCase = new Mock<ICreateQuestionsUseCase>();
        _mockCreateOptionsUseCase = new Mock<ICreateOptionsUseCase>();

        _createQuizDtoValidator = new CreateQuizDtoValidator();

        _createQuizUseCase = new CreateQuizUseCase(
            _mockQuizRepository.Object,
            _mockMapper.Object,
            _createQuizDtoValidator,
            _mockCreateQuestionsUseCase.Object,
            _mockCreateOptionsUseCase.Object
        );
    }

    [Fact]
    public async Task Execute_ValidOptions_CallsRepositoryCreateMultipleAsync()
    {
        // Arrange
        Guid classSourceId = Guid.NewGuid();
        Guid quizId = Guid.NewGuid();

        CreateQuizDto dto = new CreateQuizDto
        {
            Title = "Math Quiz",
            Description = "Generated quiz",
            Questions = []
        };

        Quiz mappedQuiz = new Quiz
        {
            Title = "Math Quiz",
            Description = "Generated quiz",
            Status = QuizStatus.draft,
            Deleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        Quiz createdQuiz = new Quiz
        {
            Id = quizId,
            Title = "Math Quiz",
            Description = "Generated quiz",
            Status = QuizStatus.draft,
            Deleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockMapper.Setup(m => m.Map<Quiz>(dto)).Returns(mappedQuiz);

        _mockQuizRepository.Setup(r => r.CreateAsync(mappedQuiz))
            .ReturnsAsync(createdQuiz);

        // Act
        await _createQuizUseCase.Execute(dto, classSourceId);

        // Assert
        _mockMapper.Verify(m => m.Map<Quiz>(dto), Times.Once);

        _mockQuizRepository.Verify(
            r => r.CreateAsync(It.Is<Quiz>(q => q.Title == dto.Title)),
            Times.Once);

        _mockCreateQuestionsUseCase.Verify(
            u => u.Execute(
                dto.Questions,
                classSourceId,
                createdQuiz.Id),
            Times.Once);

        _mockCreateOptionsUseCase.Verify(
            u => u.Execute(
                It.Is<IEnumerable<CreateOptionDto>>(options =>
                    options.Count() ==
                    dto.Questions.SelectMany(q => q.Options).Count()
                )),
            Times.Once);
    }
}
