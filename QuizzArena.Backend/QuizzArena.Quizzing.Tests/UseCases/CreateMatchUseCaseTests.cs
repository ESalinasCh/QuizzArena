using AutoMapper;
using FluentValidation;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchUseCases;
using QuizzArena.Quizzing.Application.Validators.Match;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Match = QuizzArena.Quizzing.Domain.Entities.Match;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class CreateMatchUseCaseTests
{
    // Mocks
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IQuizRepository> _mockQuizRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IQuizQuestionRepository> _mockQuizQuestionRepository;

    // Real
    private readonly CreateMatchDtoValidator _validator;

    // Target
    private readonly CreateMatchUseCase _useCase;

    public CreateMatchUseCaseTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockQuizRepository = new Mock<IQuizRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockQuizQuestionRepository = new Mock<IQuizQuestionRepository>();

        _validator = new CreateMatchDtoValidator();

        _useCase = new CreateMatchUseCase(
            _mockMatchRepository.Object,
            _validator,
            _mockMapper.Object,
            _mockQuizRepository.Object,
            _mockQuizQuestionRepository.Object
        );
    }

    private static MatchCreateDto BuildValidDto() => new()
    {
        Title = "Test Match",
        StartedAt = DateTimeOffset.UtcNow.AddHours(1),
        FinishedAt = DateTimeOffset.UtcNow.AddHours(2),
        TimeMinutes = 30,
        AttemptsAmount = 1,
        QuizId = Guid.NewGuid(),
        CourseId = Guid.NewGuid(),
        QuestionsAmount = 3
    };

    // ── Happy path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_ValidDto_ReturnsMatchCreatedResponse()
    {
        // Arrange
        var dto = BuildValidDto();
        var quiz = new Quiz { Id = dto.QuizId, Title = "Cloud Quiz" };
        var mappedMatch = new Match { Id = Guid.NewGuid() };
        var createdMatch = new Match { Id = mappedMatch.Id };
        var expectedResponse = new MatchCreatedResponseDto { Id = createdMatch.Id, QuizId = dto.QuizId };

        _mockQuizRepository.Setup(r => r.GetByIdAsync(dto.QuizId)).ReturnsAsync(quiz);
        _mockMapper.Setup(m => m.Map<Match>(dto)).Returns(mappedMatch);
        _mockMatchRepository.Setup(r => r.CreateMatchAsync(It.IsAny<Match>())).ReturnsAsync(createdMatch);
        _mockMapper.Setup(m => m.Map<MatchCreatedResponseDto>(createdMatch)).Returns(expectedResponse);
        _mockQuizQuestionRepository.Setup(m => m.GetQuestionsByQuizIdAsync(dto.QuizId)).ReturnsAsync(new List<Question>() { new(), new(), new() });
        // Act
        var result = await _useCase.Execute(dto);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task Execute_ValidDto_PersistsMatchOnce()
    {
        // Arrange
        var dto = BuildValidDto();
        var quiz = new Quiz { Id = dto.QuizId, Title = "Cloud Quiz" };
        var mappedMatch = new Match();

        _mockQuizRepository.Setup(r => r.GetByIdAsync(dto.QuizId)).ReturnsAsync(quiz);
        _mockMapper.Setup(m => m.Map<Match>(dto)).Returns(mappedMatch);
        _mockMatchRepository.Setup(r => r.CreateMatchAsync(It.IsAny<Match>())).ReturnsAsync(new Match());
        _mockMapper.Setup(m => m.Map<MatchCreatedResponseDto>(It.IsAny<Match>())).Returns(new MatchCreatedResponseDto());
        _mockQuizQuestionRepository.Setup(m => m.GetQuestionsByQuizIdAsync(dto.QuizId)).ReturnsAsync(new List<Question>() { new(), new(), new() });

        // Act
        await _useCase.Execute(dto);

        // Assert
        _mockMatchRepository.Verify(r => r.CreateMatchAsync(It.IsAny<Match>()), Times.Once);
    }

    // ── Mode always Exam ─────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_ValidDto_SetsMatchModeToExam()
    {
        // Arrange
        var dto = BuildValidDto();
        var quiz = new Quiz { Id = dto.QuizId, Title = "Cloud Quiz" };
        var mappedMatch = new Match();
        Match? capturedMatch = null;

        _mockQuizRepository.Setup(r => r.GetByIdAsync(dto.QuizId)).ReturnsAsync(quiz);
        _mockMapper.Setup(m => m.Map<Match>(dto)).Returns(mappedMatch);
        _mockMatchRepository
            .Setup(r => r.CreateMatchAsync(It.IsAny<Match>()))
            .Callback<Match>(m => capturedMatch = m)
            .ReturnsAsync(new Match());
        _mockMapper.Setup(m => m.Map<MatchCreatedResponseDto>(It.IsAny<Match>())).Returns(new MatchCreatedResponseDto());
        _mockQuizQuestionRepository.Setup(m => m.GetQuestionsByQuizIdAsync(dto.QuizId)).ReturnsAsync(new List<Question>() { new(), new(), new() });

        // Act
        await _useCase.Execute(dto);

        // Assert
        Assert.Equal(MatchMode.Exam, capturedMatch!.Mode);
    }

    // ── Code is a 6-digit string ─────────────────────────────────────────────

    [Fact]
    public async Task Execute_ValidDto_SetsCodeAsSixDigitString()
    {
        // Arrange
        var dto = BuildValidDto();
        var quiz = new Quiz { Id = dto.QuizId, Title = "Cloud Quiz" };
        var mappedMatch = new Match();
        Match? capturedMatch = null;

        _mockQuizRepository.Setup(r => r.GetByIdAsync(dto.QuizId)).ReturnsAsync(quiz);
        _mockMapper.Setup(m => m.Map<Match>(dto)).Returns(mappedMatch);
        _mockMatchRepository
            .Setup(r => r.CreateMatchAsync(It.IsAny<Match>()))
            .Callback<Match>(m => capturedMatch = m)
            .ReturnsAsync(new Match());
        _mockMapper.Setup(m => m.Map<MatchCreatedResponseDto>(It.IsAny<Match>())).Returns(new MatchCreatedResponseDto());
        _mockQuizQuestionRepository.Setup(m => m.GetQuestionsByQuizIdAsync(dto.QuizId)).ReturnsAsync(new List<Question>() { new(), new(), new() });

        // Act
        await _useCase.Execute(dto);

        // Assert
        Assert.NotNull(capturedMatch!.Code);
        Assert.Equal(6, capturedMatch.Code.Length);
        Assert.True(int.TryParse(capturedMatch.Code, out _));
    }

    // ── QuestionsAmount is always null ───────────────────────────────────────

    [Fact]
    public async Task Execute_DtoWithQuestionsAmount_SetsQuestionsAmountToCorrectValue()
    {
        // Arrange
        var dto = BuildValidDto();
        dto.QuestionsAmount = 3;
        var quiz = new Quiz { Id = dto.QuizId, Title = "Cloud Quiz" };
        var mappedMatch = new Match { QuestionsAmount = 3 };
        Match? capturedMatch = null;

        _mockQuizRepository.Setup(r => r.GetByIdAsync(dto.QuizId)).ReturnsAsync(quiz);
        _mockMapper.Setup(m => m.Map<Match>(dto)).Returns(mappedMatch);
        _mockMatchRepository
            .Setup(r => r.CreateMatchAsync(It.IsAny<Match>()))
            .Callback<Match>(m => capturedMatch = m)
            .ReturnsAsync(new Match());
        _mockMapper.Setup(m => m.Map<MatchCreatedResponseDto>(It.IsAny<Match>())).Returns(new MatchCreatedResponseDto());
        _mockQuizQuestionRepository.Setup(m => m.GetQuestionsByQuizIdAsync(dto.QuizId)).ReturnsAsync(new List<Question>() { new(), new(), new() });

        // Act
        await _useCase.Execute(dto);

        // Assert
        Assert.NotNull(capturedMatch);
        Assert.Equal(3, capturedMatch.QuestionsAmount);
    }

    // ── Audit timestamps are set ─────────────────────────────────────────────

    [Fact]
    public async Task Execute_ValidDto_SetsCreatedAtAndUpdatedAt()
    {
        // Arrange
        var dto = BuildValidDto();
        var quiz = new Quiz { Id = dto.QuizId, Title = "Cloud Quiz" };
        var mappedMatch = new Match();
        Match? capturedMatch = null;
        var before = DateTimeOffset.UtcNow;

        _mockQuizRepository.Setup(r => r.GetByIdAsync(dto.QuizId)).ReturnsAsync(quiz);
        _mockMapper.Setup(m => m.Map<Match>(dto)).Returns(mappedMatch);
        _mockMatchRepository
            .Setup(r => r.CreateMatchAsync(It.IsAny<Match>()))
            .Callback<Match>(m => capturedMatch = m)
            .ReturnsAsync(new Match());
        _mockMapper.Setup(m => m.Map<MatchCreatedResponseDto>(It.IsAny<Match>())).Returns(new MatchCreatedResponseDto());
        _mockQuizQuestionRepository.Setup(m => m.GetQuestionsByQuizIdAsync(dto.QuizId)).ReturnsAsync(new List<Question>() { new(), new(), new() });

        // Act
        await _useCase.Execute(dto);
        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.InRange(capturedMatch!.CreatedAt, before, after);
        Assert.InRange(capturedMatch.UpdatedAt, before, after);
    }

    // ── Error paths ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_InvalidDto_ThrowsValidationException()
    {
        // Arrange — FinishedAt before StartedAt makes it invalid
        var dto = BuildValidDto();
        dto.FinishedAt = dto.StartedAt.AddHours(-1);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));
    }

    [Fact]
    public async Task Execute_InvalidDto_NeverCallsRepository()
    {
        // Arrange
        var dto = BuildValidDto();
        dto.QuizId = Guid.Empty; // will fail validation

        // Act
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        // Assert
        _mockMatchRepository.Verify(r => r.CreateMatchAsync(It.IsAny<Match>()), Times.Never);
        _mockQuizRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Execute_QuizNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = BuildValidDto();

        _mockQuizRepository.Setup(r => r.GetByIdAsync(dto.QuizId)).ReturnsAsync((Quiz?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _useCase.Execute(dto));
    }

    [Fact]
    public async Task Execute_QuizNotFound_NeverCallsMatchRepository()
    {
        // Arrange
        var dto = BuildValidDto();

        _mockQuizRepository.Setup(r => r.GetByIdAsync(dto.QuizId)).ReturnsAsync((Quiz?)null);

        // Act
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _useCase.Execute(dto));

        // Assert
        _mockMatchRepository.Verify(r => r.CreateMatchAsync(It.IsAny<Match>()), Times.Never);
    }


    [Fact]
    public async Task Execute_ShouldThrowException_WhenQuestionsAmountExceedsAvailableQuestions()
    {
        var dto = BuildValidDto();
        dto.QuestionsAmount = 1000;

        _mockQuizRepository
            .Setup(x => x.GetByIdAsync(dto.QuizId))
            .ReturnsAsync(new Quiz());

        _mockMapper
            .Setup(x => x.Map<Match>(dto))
            .Returns(new Match());

        _mockQuizQuestionRepository
            .Setup(x => x.GetQuestionsByQuizIdAsync(dto.QuizId))
            .ReturnsAsync(new List<Question>
            {
            new(),
            new(),
            new()
            });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.Execute(dto));
    }
}
