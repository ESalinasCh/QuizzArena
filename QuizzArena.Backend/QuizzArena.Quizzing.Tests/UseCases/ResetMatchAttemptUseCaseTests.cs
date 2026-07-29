using Moq;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchAttemptUseCases;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class ResetMatchAttemptUseCaseTests
{
    private readonly Mock<IMatchAttemptRepository> _mockMatchAttemptRepository;
    private readonly Mock<IMatchRepository> _mockMatchRepository;

    private readonly ResetMatchAttemptUseCase _useCase;

    public ResetMatchAttemptUseCaseTests()
    {
        _mockMatchAttemptRepository = new Mock<IMatchAttemptRepository>();
        _mockMatchRepository = new Mock<IMatchRepository>();

        _useCase = new ResetMatchAttemptUseCase(
            _mockMatchAttemptRepository.Object,
            _mockMatchRepository.Object
        );
    }

    [Fact]
    public async Task Execute_UserIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        Guid matchId = Guid.NewGuid();
        Guid userId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.Execute(matchId, userId));

        Assert.Equal("userId", exception.ParamName);
        Assert.Equal("User ID cannot be empty. (Parameter 'userId')", exception.Message);
    }

    [Fact]
    public async Task Execute_WhenMatchDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        Guid matchId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(r => r.GetMatchByIdAsync(matchId))
            .ReturnsAsync((Domain.Entities.Match?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(matchId, userId));

        Assert.Equal("Match doesn't exist", exception.Message);
    }

    [Fact]
    public async Task Execute_WhenMatchAttemptsAreEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        Guid matchId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(r => r.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Domain.Entities.Match());

        _mockMatchAttemptRepository
            .Setup(r => r.GetAttemptsByUserIds(matchId, It.IsAny<List<Guid>>()))
            .ReturnsAsync([]);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(matchId, userId));

        Assert.Equal("User does not have any match attempts.", exception.Message);

        _mockMatchAttemptRepository.Verify(
            r => r.UpdateMatchAttempts(It.IsAny<List<MatchAttempt>>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_WhenMatchAttemptsExist_MarksAllAsDeletedAndUpdatesRepository()
    {
        // Arrange
        Guid matchId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(r => r.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Domain.Entities.Match());

        List<MatchAttempt> attempts =
        [
            new MatchAttempt(),
            new MatchAttempt()
        ];

        _mockMatchAttemptRepository
            .Setup(r => r.GetAttemptsByUserIds(matchId, It.IsAny<List<Guid>>()))
            .ReturnsAsync(attempts);

        // Act
        await _useCase.Execute(matchId, userId);

        // Assert
        foreach (MatchAttempt attempt in attempts)
        {
            Assert.True(attempt.Deleted);
            Assert.NotNull(attempt.DeletedAt);
            Assert.NotNull(attempt.UpdatedAt);

            Assert.True(attempt.DeletedAt <= DateTimeOffset.UtcNow);
            Assert.True(attempt.UpdatedAt <= DateTimeOffset.UtcNow);
        }

        _mockMatchAttemptRepository.Verify(r => r.UpdateMatchAttempts(attempts), Times.Once);
    }
}
