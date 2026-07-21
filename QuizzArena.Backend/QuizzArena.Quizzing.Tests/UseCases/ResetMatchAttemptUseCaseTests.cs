using Moq;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchAttemptUseCases;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class ResetMatchAttemptUseCaseTests
{
    private readonly Mock<IMatchAttemptRepository> _mockMatchAttemptRepository;

    private readonly ResetMatchAttemptUseCase _useCase;

    public ResetMatchAttemptUseCaseTests()
    {
        _mockMatchAttemptRepository = new Mock<IMatchAttemptRepository>();

        _useCase = new ResetMatchAttemptUseCase(
            _mockMatchAttemptRepository.Object
        );
    }

    [Fact]
    public async Task Execute_UserIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        Guid userId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.Execute(userId));

        Assert.Equal("userId", exception.ParamName);
        Assert.Equal("User ID cannot be empty. (Parameter 'userId')", exception.Message);
    }

    [Fact]
    public async Task Execute_WhenMatchAttemptsAreNull_ThrowsInvalidOperationException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MatchAttemptFilters filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };

        _mockMatchAttemptRepository
            .Setup(r => r.GetAttemptsByStudentId(userId, filters))
            .ReturnsAsync([]);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(userId));

        Assert.Equal("User does not have any match attempts.", exception.Message);

        _mockMatchAttemptRepository.Verify(
            r => r.UpdateMatchAttempts(It.IsAny<List<MatchAttempt>>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_WhenMatchAttemptsAreEmpty_UpdatesEmptyList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MatchAttemptFilters filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };

        List<MatchAttempt> attempts = new List<MatchAttempt>
        {
            new()
            {
                Id = Guid.NewGuid(),
                StartDateTime = DateTimeOffset.UtcNow,
            }
        };

        _mockMatchAttemptRepository
            .Setup(r => r.GetAttemptsByStudentId(userId, filters))
            .ReturnsAsync(attempts);

        // Act
        await _useCase.Execute(userId);

        // Assert
        _mockMatchAttemptRepository.Verify(r => r.UpdateMatchAttempts(attempts), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenMatchAttemptsExist_MarksAllAsDeletedAndUpdatesRepository()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        List<MatchAttempt> attempts =
        [
            new MatchAttempt(),
            new MatchAttempt()
        ];

        _mockMatchAttemptRepository
            .Setup(r => r.GetAttemptsByStudentId(userId, It.IsAny<MatchAttemptFilters>()))
            .ReturnsAsync(attempts);

        // Act
        await _useCase.Execute(userId);

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
