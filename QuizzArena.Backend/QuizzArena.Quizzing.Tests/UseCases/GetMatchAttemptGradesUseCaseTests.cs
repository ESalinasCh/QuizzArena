using AutoMapper;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchAttemptUseCases;
using QuizzArena.Quizzing.Application.Validators.FiltersValidators;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class GetMatchAttemptGradesUseCaseTests
{
    private readonly Mock<IMatchAttemptRepository> _mockMatchAttemptRepository;
    private readonly Mock<IMapper> _mockMapper;

    private readonly MatchAttemptFiltersValidator _validator;

    private readonly GetMatchAttemptGradesUseCase _getMatchAttemptGradesUseCase;

    public GetMatchAttemptGradesUseCaseTests()
    {
        _mockMatchAttemptRepository = new Mock<IMatchAttemptRepository>();
        _mockMapper = new Mock<IMapper>();

        _validator = new MatchAttemptFiltersValidator();

        _getMatchAttemptGradesUseCase = new GetMatchAttemptGradesUseCase(
            _mockMatchAttemptRepository.Object,
            _validator,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task Execute_ShouldReturnMappedAttempts_WhenBestAttemptsExist()
    {
        // Arrange
        Guid matchId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        var filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };

        var bestAttempt = new MatchAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MatchId = matchId,
            Score = 100,
            Nickname = "Test"
        };

        var otherAttempt = new MatchAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MatchId = matchId,
            Score = 80,
            Nickname = "Test"
        };

        var response = new MatchAttemptGradesResponseDto
        {
            Score = 100,
            UserId = userId,
            Nickname = "Test"
        };

        _mockMatchAttemptRepository
            .Setup(x => x.GetAttemptsByMatchId(matchId, filters))
            .ReturnsAsync([bestAttempt]);

        _mockMatchAttemptRepository
            .Setup(x => x.GetAttemptsByUserIds(matchId, new List<Guid> { userId }))
            .ReturnsAsync([bestAttempt, otherAttempt]);

        _mockMapper
            .Setup(x => x.Map<MatchAttemptGradesResponseDto>(bestAttempt))
            .Returns(response);


        // Act
        var result = await _getMatchAttemptGradesUseCase.Execute(matchId, filters);


        // Assert
        Assert.NotNull(result);

        Assert.Equal(100, result[0].Score);
        Assert.Equal(userId, result[0].UserId);

        Assert.Single(bestAttempt.OtherAttempts);
        Assert.Equal(80, bestAttempt.OtherAttempts[0].Score);

        _mockMatchAttemptRepository.Verify(
            x => x.GetAttemptsByMatchId(matchId, filters),
            Times.Once);

        _mockMatchAttemptRepository.Verify(
            x => x.GetAttemptsByUserIds(matchId, It.Is<List<Guid>>(ids => ids.Contains(userId))),
            Times.Once);

        _mockMapper.Verify(
            x => x.Map<MatchAttemptGradesResponseDto>(bestAttempt),
            Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldNotGetAttempts_WhenNoBestAttemptsExist()
    {
        // Arrange
        Guid matchId = Guid.NewGuid();

        var filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };


        _mockMatchAttemptRepository
            .Setup(x => x.GetAttemptsByMatchId(matchId, filters))
            .ReturnsAsync([]);


        // Act
        var result = await _getMatchAttemptGradesUseCase.Execute(matchId, filters);


        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _mockMatchAttemptRepository.Verify(
            x => x.GetAttemptsByUserIds(
                It.IsAny<Guid>(),
                It.IsAny<List<Guid>>()),
            Times.Never);

        _mockMapper.Verify(
            x => x.Map<MatchAttemptGradesResponseDto>(
                It.IsAny<MatchAttempt>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldGetOtherAttempts()
    {
        // Arrange
        Guid matchId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        var filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };
        var bestAttempt = new MatchAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Score = 100
        };
        var lowerScore = new MatchAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Score = 50
        };
        var higherScore = new MatchAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Score = 90
        };

        _mockMatchAttemptRepository
            .Setup(x => x.GetAttemptsByMatchId(matchId, filters))
            .ReturnsAsync([bestAttempt]);

        _mockMatchAttemptRepository
            .Setup(x => x.GetAttemptsByUserIds(matchId, new List<Guid> { userId }))
            .ReturnsAsync([bestAttempt, lowerScore, higherScore]);

        _mockMapper
            .Setup(x => x.Map<MatchAttemptGradesResponseDto>(It.IsAny<MatchAttempt>()))
            .Returns(new MatchAttemptGradesResponseDto());

        // Act
        await _getMatchAttemptGradesUseCase.Execute(matchId, filters);

        // Assert
        Assert.Equal(2, bestAttempt.OtherAttempts.Count);
        Assert.Equal(90, bestAttempt.OtherAttempts[0].Score);
        Assert.Equal(50, bestAttempt.OtherAttempts[1].Score);
    }
}
