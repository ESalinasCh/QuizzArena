using Moq;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.UseCases;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class GetMatchesUseCaseTests
{
    private readonly Mock<ICourseContract> _mockCourseImpl;
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<ICurrentUser> _mockCurrentUser;

    private readonly GetMatchesUseCase _getMatchesUseCase;

    public GetMatchesUseCaseTests()
    {
        _mockCourseImpl = new Mock<ICourseContract>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();

        _getMatchesUseCase = new GetMatchesUseCase(
            _mockCourseImpl.Object,
            _mockMatchRepository.Object,
            _mockCurrentUser.Object
        );
    }

    [Fact]
    public async Task GetMatches_ValidRequest_ReturnsMatches()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var courses = new List<CourseSummaryDTO>
        {
            new CourseSummaryDTO { Id = Guid.NewGuid(), CourseName = "Course 1" },
            new CourseSummaryDTO { Id = Guid.NewGuid(), CourseName = "Course 2" }
        };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(courses);
        var matches = new List<Domain.Entities.Match>
        {
            new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = courses[0].Id },
            new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = courses[1].Id }
        };
        _mockMatchRepository.Setup(m => m.GetMatchesAsync(It.IsAny<List<Guid>>(), It.IsAny<MatchQueryParametersDto>())).ReturnsAsync(matches);
        var query = new MatchQueryParametersDto();

        // Act
        List<MatchResponseDto> result = await _getMatchesUseCase.Execute(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Course 1", result[0].CourseName);
        Assert.Equal("Course 2", result[1].CourseName);
    }

}
