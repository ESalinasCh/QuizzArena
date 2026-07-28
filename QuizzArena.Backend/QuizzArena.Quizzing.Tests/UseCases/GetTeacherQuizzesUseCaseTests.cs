using AutoMapper;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.QuizUseCases;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class GetTeacherQuizzesUseCaseTests
{
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IQuizQueriesRepository> _mockQuizRepo;

    private readonly GetTeacherQuizzesUseCase _useCase;

    public GetTeacherQuizzesUseCaseTests()
    {
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockMapper = new Mock<IMapper>();
        _mockQuizRepo = new Mock<IQuizQueriesRepository>();

        _useCase = new GetTeacherQuizzesUseCase(
            _mockCurrentUser.Object,
            _mockMapper.Object,
            _mockQuizRepo.Object
        );
    }

    [Fact]
    public async Task Execute_NoQuizzes_ReturnsEmptyList()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        _mockCurrentUser.Setup(c => c.UserId).Returns(teacherId.ToString());
        _mockQuizRepo.Setup(r => r.GetByTeacherIdAsync(teacherId, null)).ReturnsAsync([]);
        _mockMapper.Setup(m => m.Map<List<TeacherQuizResponseDto>>(It.IsAny<List<Quiz>>())).Returns([]);

        // Act
        List<TeacherQuizResponseDto> result = await _useCase.Execute(null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Execute_QuizzesFound_ReturnsMappedDtos()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        _mockCurrentUser.Setup(c => c.UserId).Returns(teacherId.ToString());
        var quizzes = new List<Quiz>
        {
            new Quiz { Id = Guid.NewGuid(), Title = "Math Basics", TeacherId = teacherId }
        };
        var expected = new List<TeacherQuizResponseDto>
        {
            new TeacherQuizResponseDto { Id = quizzes[0].Id, Title = "Math Basics" }
        };
        _mockQuizRepo.Setup(r => r.GetByTeacherIdAsync(teacherId, null)).ReturnsAsync(quizzes);
        _mockMapper.Setup(m => m.Map<List<TeacherQuizResponseDto>>(quizzes)).Returns(expected);

        // Act
        List<TeacherQuizResponseDto> result = await _useCase.Execute(null);

        // Assert
        Assert.Single(result);
        Assert.Equal(expected[0].Id, result[0].Id);
        Assert.Equal("Math Basics", result[0].Title);
    }

    [Fact]
    public async Task Execute_WithOrigin_PassesOriginToRepository()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        _mockCurrentUser.Setup(c => c.UserId).Returns(teacherId.ToString());
        _mockQuizRepo.Setup(r => r.GetByTeacherIdAsync(teacherId, QuizOrigin.ManuallyCreated)).ReturnsAsync([]);
        _mockMapper.Setup(m => m.Map<List<TeacherQuizResponseDto>>(It.IsAny<List<Quiz>>())).Returns([]);

        // Act
        await _useCase.Execute(QuizOrigin.ManuallyCreated);

        // Assert
        _mockQuizRepo.Verify(r => r.GetByTeacherIdAsync(teacherId, QuizOrigin.ManuallyCreated), Times.Once);
    }

    [Fact]
    public async Task Execute_UserIdInvalid_ThrowsFormatException()
    {
        // Arrange
        _mockCurrentUser.Setup(c => c.UserId).Returns("not-a-guid");

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _useCase.Execute(null));
        _mockQuizRepo.Verify(r => r.GetByTeacherIdAsync(It.IsAny<Guid>(), It.IsAny<QuizOrigin?>()), Times.Never);
    }
}
