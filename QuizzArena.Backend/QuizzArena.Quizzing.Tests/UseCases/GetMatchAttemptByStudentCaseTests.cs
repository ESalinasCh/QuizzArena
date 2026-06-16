
using Moq;
using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.Validators.FiltersValidators;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

using Shared.Contracts;
using Shared.Contracts.DTOs;
using FluentValidation;


namespace QuizzArena.Quizzing.Tests.UseCases;

public class GetMatchAttemptByStudentCaseTests
{
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Mock<IMatchQueriesRepository > _mockMatchRepository;
    private readonly Mock<ICourseContract> _mockCourseContract;

    private readonly GetMatchAttemptsByStudent _useCase;

    public GetMatchAttemptByStudentCaseTests()
    {
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockMatchRepository = new Mock<IMatchQueriesRepository>();
        _mockCourseContract = new Mock<ICourseContract>();

        _useCase = new GetMatchAttemptsByStudent(
            _mockCurrentUser.Object,
            _mockMatchRepository.Object,
            _mockCourseContract.Object,
            new MatchAttemptFiltersValidator()
        );
    }

    [Fact]
    public async Task Execute_InvalidUserId_ThrowsFormatException()
    {
        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns("invalid-guid");

        var filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };

        await Assert.ThrowsAsync<FormatException>(
            () => _useCase.Execute(filters));
    }

    [Fact]
    public async Task Execute_InvalidFilters_ThrowsValidationException()
    {
        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        var filters = new MatchAttemptFilters
        {
            Page = 0,
            PageSize = 10
        };

        await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.Execute(filters));
    }

    [Fact]
    public async Task Execute_NoMatchAttempts_ReturnsEmptyList()
    {
        Guid studentId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(studentId.ToString());

        var filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };

        _mockMatchRepository
            .Setup(x => x.GetAttemptsByStudentId(studentId, filters))
            .ReturnsAsync([]);

        _mockMatchRepository
            .Setup(x => x.GetMatchesByIds(It.IsAny<List<Guid>>()))
            .ReturnsAsync([]);

        _mockCourseContract
            .Setup(x => x.GetCoursesByIds(It.IsAny<List<Guid>>()))
            .ReturnsAsync([]);

        List<GetMatchAttemptDTO> result = await _useCase.Execute(filters);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Execute_ValidData_ReturnsMappedMatchAttempts()
    {
        Guid studentId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid matchId = Guid.NewGuid();
        Guid courseId = Guid.NewGuid();

        DateTime startedAt = DateTime.UtcNow.AddMinutes(-30);
        DateTime completedAt = DateTime.UtcNow;

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(studentId.ToString());

        var filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };

        var attempts = new List<MatchAttempt>
        {
            new()
            {
                Id = attemptId,
                MatchId = matchId,
                StartDateTime = startedAt,
                EndDateTime = completedAt,
                Score = 85,
                Status = QuizAttemptStatus.Completed
            }
        };

        var matches = new List<Domain.Entities.Match>
        {
            new()
            {
                Id = matchId,
                CourseId = courseId,
                TimeMinutes = 20
            }
        };

        var courses = new List<CourseSummaryDTO>
        {
            new()
            {
                Id = courseId,
                CourseName = "Algorithms"
            }
        };

        _mockMatchRepository
            .Setup(x => x.GetAttemptsByStudentId(studentId, filters))
            .ReturnsAsync(attempts);

        _mockMatchRepository
            .Setup(x => x.GetMatchesByIds(It.Is<List<Guid>>(ids =>
                ids.Count == 1 &&
                ids.Contains(matchId))))
            .ReturnsAsync(matches);

        _mockCourseContract
            .Setup(x => x.GetCoursesByIds(It.Is<List<Guid>>(ids =>
                ids.Count == 1 &&
                ids.Contains(courseId))))
            .ReturnsAsync(courses);

        List<GetMatchAttemptDTO> result = await _useCase.Execute(filters);

        Assert.Single(result);

        var item = result.Single();

        Assert.Equal(attemptId, item.Id);
        Assert.Equal("Algorithms", item.CourseName);
        Assert.Equal(startedAt, item.StartedAt);
        Assert.Equal(completedAt, item.CompletedAt);
        Assert.Equal(85, item.Score);
        Assert.Equal(QuizAttemptStatus.Completed, item.Status);
        Assert.Equal(20, item.Duration);
    }
}
