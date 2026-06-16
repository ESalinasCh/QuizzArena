using FluentValidation;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases;
using QuizzArena.Quizzing.Application.UseCases.MatchUseCases;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class GetMatchesUseCaseTests
{
    private readonly Mock<IValidator<MatchQueryParametersDto>> _mockQueryValidator;
    private readonly Mock<ICourseContract> _mockCourseImpl;
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<ICurrentUser> _mockCurrentUser;

    private readonly GetMatchesUseCase _getMatchesUseCase;

    public GetMatchesUseCaseTests()
    {
        _mockQueryValidator = new Mock<IValidator<MatchQueryParametersDto>>();
        _mockCourseImpl = new Mock<ICourseContract>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();

        _getMatchesUseCase = new GetMatchesUseCase(
            _mockQueryValidator.Object,
            _mockCourseImpl.Object,
            _mockMatchRepository.Object,
            _mockCurrentUser.Object
        );
    }

    [Fact]
    public async Task GetMatches_NoCourses_ReturnsEmptyList()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var courses = new List<CourseSummaryDTO>();
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(courses);
        var matches = new List<Domain.Entities.Match>();
        _mockMatchRepository.Setup(m => m.GetMatchesAsync(
            It.IsAny<List<Guid>>(),
            It.IsAny<MatchQueryParametersDto>())
        ).ReturnsAsync(matches);
        var query = new MatchQueryParametersDto();

        // Act
        List<MatchResponseDto> result = await _getMatchesUseCase.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMatches_MatchCourseNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var courses = new List<CourseSummaryDTO>
        {
            new CourseSummaryDTO { Id = Guid.NewGuid(), CourseName = "Electricity", ProfessorName = "Nikola Tesla" }
        };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(courses);
        var matches = new List<Domain.Entities.Match>
        {
            new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = Guid.NewGuid() }
        };
        _mockMatchRepository.Setup(m => m.GetMatchesAsync(
            It.IsAny<List<Guid>>(),
            It.IsAny<MatchQueryParametersDto>())
        ).ReturnsAsync(matches);
        var query = new MatchQueryParametersDto();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _getMatchesUseCase.Execute(query));
    }

    [Fact]
    public async Task GetMatches_UserIdInvalid_ThrowsFormatException()
    {
        // Arrange
        _mockCurrentUser.Setup(c => c.UserId).Returns("not-a-guid");
        var query = new MatchQueryParametersDto();

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _getMatchesUseCase.Execute(query));
    }

    [Fact]
    public async Task GetMatches_MatchesMapped_TitleIsAaaa()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var courses = new List<CourseSummaryDTO>
        {
            new CourseSummaryDTO { Id = Guid.NewGuid(), CourseName = "Electricity", ProfessorName = "Nikola Tesla" }
        };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(courses);
        var matches = new List<Domain.Entities.Match>
        {
            new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = courses[0].Id}
        };
        _mockMatchRepository.Setup(m => m.GetMatchesAsync(
            It.IsAny<List<Guid>>(),
            It.IsAny<MatchQueryParametersDto>())
        ).ReturnsAsync(matches);
        var query = new MatchQueryParametersDto();

        // Act
        List<MatchResponseDto> result = await _getMatchesUseCase.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("aaaa", result[0].Title);
        Assert.Equal("Electricity", result[0].CourseName);
    }

    [Fact]
    public async Task GetMatches_EmptyQuery_ReturnsMatches()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var courses = new List<CourseSummaryDTO>
        {
            new CourseSummaryDTO { Id = Guid.NewGuid(), CourseName = "Electricity", ProfessorName = "Nikola Tesla" },
            new CourseSummaryDTO { Id = Guid.NewGuid(), CourseName = "Physics", ProfessorName = "Albert Einstein" }
        };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(courses);
        var matches = new List<Domain.Entities.Match>
        {
            new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = courses[0].Id },
            new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = courses[1].Id }
        };
        _mockMatchRepository.Setup(m => m.GetMatchesAsync(
            It.IsAny<List<Guid>>(),
            It.IsAny<MatchQueryParametersDto>())
        ).ReturnsAsync(matches);
        var query = new MatchQueryParametersDto();

        // Act
        List<MatchResponseDto> result = await _getMatchesUseCase.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Electricity", result[0].CourseName);
        Assert.Equal("Physics", result[1].CourseName);
    }

}
