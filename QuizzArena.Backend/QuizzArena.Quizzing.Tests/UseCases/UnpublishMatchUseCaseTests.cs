using Moq;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchUseCases;
using QuizzArena.Quizzing.Domain.Enums;
using Match = QuizzArena.Quizzing.Domain.Entities.Match;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class UnpublishMatchUseCaseTests
{
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IMatchAttemptRepository> _mockMatchAttemptRepository;

    private readonly UnpublishMatchUseCase _useCase;

    public UnpublishMatchUseCaseTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockMatchAttemptRepository = new Mock<IMatchAttemptRepository>();

        _useCase = new UnpublishMatchUseCase(
            _mockMatchRepository.Object,
            _mockMatchAttemptRepository.Object);
    }

    [Fact]
    public async Task Execute_MatchDoesNotExist_ThrowsInvalidOperationException()
    {
        Guid matchId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync((Match?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(matchId));
    }

    [Fact]
    public async Task Execute_MatchAlreadyPending_ThrowsInvalidOperationException()
    {
        Guid matchId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Match
            {
                Id = matchId,
                Status = MatchStatus.Pending
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(matchId));
    }

    [Fact]
    public async Task Execute_AttemptsInProgress_ThrowsInvalidOperationException()
    {
        Guid matchId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Match
            {
                Id = matchId,
                Status = MatchStatus.Active
            });

        _mockMatchAttemptRepository
            .Setup(x => x.GetMatchAttemptCountByMatchIdAndStatusAsync(matchId, QuizAttemptStatus.InProgress))
            .ReturnsAsync(2);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(matchId));
    }

    [Fact]
    public async Task Execute_ValidMatch_UnpublishesMatchAndReturnsResponse()
    {
        Guid matchId = Guid.NewGuid();

        Match match = new()
        {
            Id = matchId,
            Status = MatchStatus.Active,
            StartedAt = DateTimeOffset.UtcNow.AddHours(1),
            FinishedAt = DateTimeOffset.UtcNow.AddHours(2),
            Code = "ABC123"
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);

        _mockMatchAttemptRepository
            .Setup(x => x.GetMatchAttemptCountByMatchIdAndStatusAsync(matchId, QuizAttemptStatus.InProgress))
            .ReturnsAsync(0);

        MatchPublicationResponseDto response = await _useCase.Execute(matchId);

        Assert.Equal(matchId, response.Id);
        Assert.Equal(MatchStatus.Pending, response.PublicationStatus);
        Assert.Equal(match.StartedAt, response.StartDate);
        Assert.Equal(match.FinishedAt, response.EndDate);
        Assert.Equal(match.Code, response.ShareCode);

        Assert.Equal(MatchStatus.Pending, match.Status);

        _mockMatchRepository.Verify(
            x => x.UpdateMatchAsync(It.Is<Match>(m => m.Status == MatchStatus.Pending)),
            Times.Once);

        _mockMatchAttemptRepository.Verify(
            x => x.GetMatchAttemptCountByMatchIdAndStatusAsync(matchId, QuizAttemptStatus.InProgress),
            Times.Once);
    }
}
