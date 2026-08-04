using Moq;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchUseCases;
using QuizzArena.Quizzing.Application.Validators.Match;
using QuizzArena.Quizzing.Domain.Entities;
using Match = QuizzArena.Quizzing.Domain.Entities.Match;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class UpdateMatchUseCaseTests
{
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IQuizQuestionRepository> _mockQuizQuestionRepository;
    private readonly UpdateMatchUseCase _useCase;

    public UpdateMatchUseCaseTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockQuizQuestionRepository = new Mock<IQuizQuestionRepository>();

        _useCase = new UpdateMatchUseCase(
            _mockMatchRepository.Object,
            _mockQuizQuestionRepository.Object,
            new UpdateMatchDtoValidator(),
            new UpdateMatchValidator());
    }
    [Fact]
    public async Task Execute_ShouldUpdateMatch_WhenValidDataIsProvided()
    {
        var matchId = Guid.NewGuid();

        var match = new Match
        {
            Id = matchId,
            Title = "Old title",
            TimeMinutes = 30,
            AttemptsAmount = 1,
            CourseId = Guid.NewGuid()
        };

        var dto = new MatchUpdateDto
        {
            Title = "New title",
            TimeMinutes = 60,
            AttemptsAmount = 3
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);

        _mockMatchRepository
            .Setup(x => x.UpdateMatchAsync(It.IsAny<Match>()))
           .ReturnsAsync((Match match) => match);

        var result = await _useCase.Execute(matchId, dto);

        Assert.Equal("New title", result.Title);
        Assert.Equal(60, result.TimeMinutes);
        Assert.Equal(3, result.AttemptsAmount);

        _mockMatchRepository.Verify(
            x => x.UpdateMatchAsync(It.Is<Match>(m =>
                m.Title == "New title" &&
                m.TimeMinutes == 60 &&
                m.AttemptsAmount == 3)),
            Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldKeepExistingValues_WhenFieldsAreNotProvided()
    {
        var matchId = Guid.NewGuid();

        var match = new Match
        {
            Id = matchId,
            Title = "Original title",
            TimeMinutes = 30,
            AttemptsAmount = 2,
            CourseId = Guid.NewGuid()
        };

        var dto = new MatchUpdateDto
        {
            Title = "Updated title"
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);

        var result = await _useCase.Execute(matchId, dto);

        Assert.Equal("Updated title", result.Title);
        Assert.Equal(30, result.TimeMinutes);
        Assert.Equal(2, result.AttemptsAmount);
    }
    [Fact]
    public async Task Execute_ShouldThrowException_WhenMatchDoesNotExist()
    {
        var matchId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync((Match?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.Execute(matchId, new MatchUpdateDto()
            {
                Title = "Match Test"
            }));
    }

    [Fact]
    public async Task Execute_ShouldUpdateQuestionsAmount_WhenEnoughQuestionsExist()
    {
        var matchId = Guid.NewGuid();

        var match = new Match
        {
            Id = matchId,
            QuizId = Guid.NewGuid(),
            QuestionsAmount = 5,
            TimeMinutes = 10,
            CourseId = Guid.NewGuid()
        };

        var dto = new MatchUpdateDto
        {
            QuestionsAmount = 3
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);

        _mockQuizQuestionRepository
            .Setup(x => x.GetQuestionsByQuizIdAsync(match.QuizId))
            .ReturnsAsync(new List<Question>
            {
            new(),
            new(),
            new(),
            new()
            });

        var result = await _useCase.Execute(matchId, dto);

        Assert.Equal(3, result.QuestionsAmount);
    }
    [Fact]
    public async Task Execute_ShouldThrowException_WhenQuestionsAmountExceedsAvailableQuestions()
    {
        var matchId = Guid.NewGuid();

        var match = new Match
        {
            Id = matchId,
            QuizId = Guid.NewGuid()
        };

        var dto = new MatchUpdateDto
        {
            QuestionsAmount = 10
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);

        _mockQuizQuestionRepository
            .Setup(x => x.GetQuestionsByQuizIdAsync(match.QuizId))
            .ReturnsAsync(new List<Question>
            {
            new(),
            new(),
            new()
            });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.Execute(matchId, dto));
    }
}
