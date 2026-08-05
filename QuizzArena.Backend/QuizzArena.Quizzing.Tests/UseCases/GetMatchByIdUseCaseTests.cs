using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchUseCases;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.DTOs;
using Match = QuizzArena.Quizzing.Domain.Entities.Match;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class GetMatchByIdUseCaseTests
{
    // Mocks
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<ICourseContract> _mockCourseContract;
    private readonly Mock<ICurrentUser> _mockCurrentUser;

    // Real: exercises the actual Match -> MatchDetailResponseDto profile
    private readonly IMapper _mapper;

    // Target
    private readonly GetMatchByIdUseCase _useCase;

    private readonly Guid _teacherId = Guid.NewGuid();

    public GetMatchByIdUseCaseTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockCourseContract = new Mock<ICourseContract>();
        _mockCurrentUser = new Mock<ICurrentUser>();

        _mapper = new MapperConfiguration(
            cfg => cfg.AddMaps(typeof(GetMatchByIdUseCase).Assembly),
            NullLoggerFactory.Instance
        ).CreateMapper();

        _mockCurrentUser.Setup(user => user.UserId).Returns(_teacherId.ToString());

        _useCase = new GetMatchByIdUseCase(
            _mockMatchRepository.Object,
            _mockCourseContract.Object,
            _mockCurrentUser.Object,
            _mapper
        );
    }

    private static Match BuildMatch(Guid matchId, Guid courseId) => new()
    {
        Id = matchId,
        Code = "ABC123",
        Title = "Docker Match",
        Status = MatchStatus.Active,
        Mode = MatchMode.Exam,
        StartedAt = DateTimeOffset.UtcNow.AddHours(1),
        FinishedAt = DateTimeOffset.UtcNow.AddHours(2),
        TimeMinutes = 30,
        QuestionsAmount = 10,
        AttemptsAmount = 2,
        ShuffleQuestion = true,
        ShuffleOptions = true,
        CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        CourseId = courseId,
        QuizId = Guid.NewGuid()
    };

    [Fact]
    public async Task Execute_MatchDoesNotExist_ThrowsKeyNotFoundException()
    {
        Guid matchId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync((Match?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _useCase.Execute(matchId));
    }

    [Fact]
    public async Task Execute_MatchBelongsToAnotherTeacherCourse_ThrowsUnauthorizedAccessException()
    {
        Guid matchId = Guid.NewGuid();
        Match match = BuildMatch(matchId, Guid.NewGuid());

        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockCourseContract
            .Setup(course => course.GetCoursesByTeacherId(_teacherId))
            .ReturnsAsync([new CourseSummaryDTO { Id = Guid.NewGuid(), CourseName = "Other course" }]);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.Execute(matchId));
    }

    [Fact]
    public async Task Execute_MatchInTeacherCourse_ReturnsMappedDetail()
    {
        Guid matchId = Guid.NewGuid();
        Guid courseId = Guid.NewGuid();
        Match match = BuildMatch(matchId, courseId);

        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockCourseContract
            .Setup(course => course.GetCoursesByTeacherId(_teacherId))
            .ReturnsAsync([new CourseSummaryDTO { Id = courseId, CourseName = "Docker course" }]);

        MatchDetailResponseDto result = await _useCase.Execute(matchId);

        Assert.Equal(match.Id, result.Id);
        Assert.Equal(match.Code, result.Code);
        Assert.Equal(match.Title, result.Title);
        Assert.Equal(match.Status, result.Status);
        Assert.Equal(match.Mode, result.Mode);
        Assert.Equal(match.StartedAt, result.StartedAt);
        Assert.Equal(match.FinishedAt, result.FinishedAt);
        Assert.Equal(match.AttemptsAmount, result.AttemptsAmount);
        Assert.Equal(match.ShuffleQuestion, result.ShuffleQuestion);
        Assert.Equal(match.ShuffleOptions, result.ShuffleOptions);
        Assert.Equal(match.CreatedAt, result.CreatedAt);
        Assert.Equal(match.CourseId, result.CourseId);
        Assert.Equal(match.QuizId, result.QuizId);

        // Renamed members: these silently default to 0 if the profile loses them
        Assert.Equal(match.TimeMinutes, result.Duration);
        Assert.Equal(match.QuestionsAmount, result.QuestionCount);
    }

    [Fact]
    public async Task Execute_MatchWithoutQuestionsAmount_ReturnsNullQuestionCount()
    {
        Guid matchId = Guid.NewGuid();
        Guid courseId = Guid.NewGuid();
        Match match = BuildMatch(matchId, courseId);
        match.QuestionsAmount = null;

        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);
        _mockCourseContract
            .Setup(course => course.GetCoursesByTeacherId(_teacherId))
            .ReturnsAsync([new CourseSummaryDTO { Id = courseId, CourseName = "Docker course" }]);

        MatchDetailResponseDto result = await _useCase.Execute(matchId);

        Assert.Null(result.QuestionCount);
    }
}
