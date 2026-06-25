using AutoMapper;
using FluentValidation;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.QuizUseCases;
using QuizzArena.Quizzing.Application.Validators.Quiz;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums; // QuizStatus used in BuildValidDto

namespace QuizzArena.Quizzing.Tests.UseCases;

public class CreateExamUseCaseTests
{
    // Mocks
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IQuizRepository> _mockQuizRepository;
    private readonly Mock<IMapper> _mockMapper;

    // Real
    private readonly CreateExamDtoValidator _validator;

    // Target
    private readonly CreateExamUseCase _useCase;

    public CreateExamUseCaseTests()
    {
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockQuizRepository = new Mock<IQuizRepository>();
        _mockMapper = new Mock<IMapper>();

        _validator = new CreateExamDtoValidator();

        _useCase = new CreateExamUseCase(
            _mockQuestionRepository.Object,
            _mockQuizRepository.Object,
            _mockMapper.Object,
            _validator
        );
    }

    private static CreateExamDto BuildValidDto(params Guid[] questionIds) => new()
    {
        Title = "Docker Quiz",
        Description = "Questions about infrastructure",
        Status = QuizStatus.draft,
        QuestionIds = questionIds.Length > 0 ? questionIds : [Guid.NewGuid()]
    };

    // ── Happy path ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_ValidDto_ReturnsCreatedQuizResponse()
    {
        // Arrange
        Guid q1 = Guid.NewGuid();
        Guid q2 = Guid.NewGuid();
        Guid quizId = Guid.NewGuid();

        CreateExamDto dto = BuildValidDto(q1, q2);

        List<Question> foundQuestions =
        [
            new Question { Id = q1 },
            new Question { Id = q2 }
        ];

        Quiz mappedQuiz = new() { Id = quizId, Title = dto.Title, Description = dto.Description };
        Quiz createdQuiz = new() { Id = quizId, Title = dto.Title, Description = dto.Description };
        CreateQuizResponseDto expectedResponse = new() { Id = quizId, Title = dto.Title };

        _mockQuestionRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(foundQuestions);

        _mockMapper.Setup(m => m.Map<Quiz>(dto)).Returns(mappedQuiz);

        _mockQuizRepository
            .Setup(r => r.CreateAsync(It.IsAny<Quiz>()))
            .ReturnsAsync(createdQuiz);

        _mockMapper.Setup(m => m.Map<CreateQuizResponseDto>(createdQuiz)).Returns(expectedResponse);

        // Act
        CreateQuizResponseDto result = await _useCase.Execute(dto);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task Execute_ValidDto_FetchesQuestionsByProvidedIds()
    {
        // Arrange
        Guid q1 = Guid.NewGuid();
        Guid q2 = Guid.NewGuid();

        CreateExamDto dto = BuildValidDto(q1, q2);

        List<Question> foundQuestions =
        [
            new Question { Id = q1 },
            new Question { Id = q2 }
        ];

        _mockQuestionRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(foundQuestions);

        _mockMapper.Setup(m => m.Map<Quiz>(dto)).Returns(new Quiz { Id = Guid.NewGuid() });

        _mockQuizRepository
            .Setup(r => r.CreateAsync(It.IsAny<Quiz>()))
            .ReturnsAsync(new Quiz());

        _mockMapper.Setup(m => m.Map<CreateQuizResponseDto>(It.IsAny<Quiz>()))
            .Returns(new CreateQuizResponseDto());

        // Act
        await _useCase.Execute(dto);

        // Assert
        _mockQuestionRepository.Verify(
            r => r.GetByIdsAsync(It.Is<List<Guid>>(ids =>
                ids.Contains(q1) && ids.Contains(q2))),
            Times.Once);
    }

    [Fact]
    public async Task Execute_ValidDto_PersistsQuizOnce()
    {
        // Arrange
        Guid q1 = Guid.NewGuid();
        CreateExamDto dto = BuildValidDto(q1);

        _mockQuestionRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([new Question { Id = q1 }]);

        _mockMapper.Setup(m => m.Map<Quiz>(dto)).Returns(new Quiz { Id = Guid.NewGuid() });

        _mockQuizRepository
            .Setup(r => r.CreateAsync(It.IsAny<Quiz>()))
            .ReturnsAsync(new Quiz());

        _mockMapper.Setup(m => m.Map<CreateQuizResponseDto>(It.IsAny<Quiz>()))
            .Returns(new CreateQuizResponseDto());

        // Act
        await _useCase.Execute(dto);

        // Assert
        _mockQuizRepository.Verify(r => r.CreateAsync(It.IsAny<Quiz>()), Times.Once);
    }

    // ── Validation failures ─────────────────────────────────────────────────

    [Fact]
    public async Task Execute_EmptyTitle_ThrowsValidationException()
    {
        CreateExamDto dto = BuildValidDto();
        dto.Title = string.Empty;

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));
    }

    [Fact]
    public async Task Execute_EmptyDescription_ThrowsValidationException()
    {
        CreateExamDto dto = BuildValidDto();
        dto.Description = string.Empty;

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));
    }

    [Fact]
    public async Task Execute_EmptyQuestionIds_ThrowsValidationException()
    {
        CreateExamDto dto = BuildValidDto();
        dto.QuestionIds = [];

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));
    }

    [Fact]
    public async Task Execute_InvalidDto_DoesNotCallRepository()
    {
        CreateExamDto dto = BuildValidDto();
        dto.Title = string.Empty;

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        _mockQuestionRepository.Verify(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()), Times.Never);
        _mockQuizRepository.Verify(r => r.CreateAsync(It.IsAny<Quiz>()), Times.Never);
    }

    // ── Domain rule: question IDs must exist ────────────────────────────────

    [Fact]
    public async Task Execute_SomeQuestionIdsNotFound_ThrowsValidationException()
    {
        // Arrange — DTO sends 3 IDs but repository only finds 2
        Guid q1 = Guid.NewGuid();
        Guid q2 = Guid.NewGuid();
        Guid q3 = Guid.NewGuid();

        CreateExamDto dto = BuildValidDto(q1, q2, q3);

        _mockQuestionRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([new Question { Id = q1 }, new Question { Id = q2 }]);

        // Act & Assert
        ValidationException ex = await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.Execute(dto));

        Assert.Contains("One or more question IDs do not exist", ex.Message);
    }

    [Fact]
    public async Task Execute_SomeQuestionIdsNotFound_DoesNotPersistQuiz()
    {
        Guid q1 = Guid.NewGuid();
        Guid q2 = Guid.NewGuid();

        CreateExamDto dto = BuildValidDto(q1, q2);

        _mockQuestionRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([new Question { Id = q1 }]); // only 1 of 2 found

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        _mockQuizRepository.Verify(r => r.CreateAsync(It.IsAny<Quiz>()), Times.Never);
    }
}
