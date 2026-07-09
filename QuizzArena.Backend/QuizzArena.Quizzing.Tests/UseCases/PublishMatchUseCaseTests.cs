using Moq;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchUseCases;
using QuizzArena.Quizzing.Domain.Enums;
using Match = QuizzArena.Quizzing.Domain.Entities.Match;


namespace QuizzArena.Quizzing.Tests.UseCases;

public class PublishMatchUseCaseTests
{
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly PublishMatchUseCase _useCase;

    public PublishMatchUseCaseTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();

        _useCase = new PublishMatchUseCase(_mockMatchRepository.Object);
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
    public async Task Execute_MatchAlreadyPublished_ThrowsInvalidOperationException()
    {
        Guid matchId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Match
            {
                Id = matchId,
                Status = MatchStatus.Active,
                StartedAt = DateTimeOffset.UtcNow.AddHours(1)
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(matchId));
    }

    [Fact]
    public async Task Execute_StartedAtIsInPast_ThrowsInvalidOperationException()
    {
        Guid matchId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Match
            {
                Id = matchId,
                Status = MatchStatus.Pending,
                StartedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(matchId));
    }

    [Fact]
    public async Task Execute_StartedAtIsGreaterThanFinishedAt_ThrowsInvalidOperationException()
    {
        Guid matchId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Match
            {
                Id = matchId,
                Status = MatchStatus.Pending,
                StartedAt = DateTimeOffset.UtcNow.AddHours(2),
                FinishedAt = DateTimeOffset.UtcNow.AddHours(1)
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(matchId));
    }

    [Fact]
    public async Task Execute_ValidMatch_PublishesMatchAndReturnsResponse()
    {
        Guid matchId = Guid.NewGuid();

        Match match = new()
        {
            Id = matchId,
            Status = MatchStatus.Pending,
            StartedAt = DateTimeOffset.UtcNow.AddHours(1),
            FinishedAt = DateTimeOffset.UtcNow.AddHours(2),
            Code = "ABC123"
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);

        MatchPublicationResponseDto response = await _useCase.Execute(matchId);

        Assert.Equal(matchId, response.Id);
        Assert.Equal(MatchStatus.Active, response.PublicationStatus);
        Assert.Equal(match.StartedAt, response.StartDate);
        Assert.Equal(match.FinishedAt, response.EndDate);
        Assert.Equal(match.Code, response.ShareCode);

        Assert.Equal(MatchStatus.Active, match.Status);

        _mockMatchRepository.Verify(
            x => x.UpdateMatchAsync(It.Is<Match>(m => m.Status == MatchStatus.Active)),
            Times.Once);
    }

}
