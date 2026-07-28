using AutoMapper;
using FluentValidation;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.QuizUseCases;
using QuizzArena.Quizzing.Application.Validators.FiltersValidators;
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
            new QuizQueryParametersValidator(),
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
        _mockQuizRepo.Setup(r => r.GetByTeacherIdAsync(teacherId, It.IsAny<QuizQueryParametersDto>())).ReturnsAsync([]);
        _mockMapper.Setup(m => m.Map<List<TeacherQuizResponseDto>>(It.IsAny<List<Quiz>>())).Returns([]);

        // Act
        List<TeacherQuizResponseDto> result = await _useCase.Execute(new QuizQueryParametersDto());

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
        _mockQuizRepo.Setup(r => r.GetByTeacherIdAsync(teacherId, It.IsAny<QuizQueryParametersDto>())).ReturnsAsync(quizzes);
        _mockMapper.Setup(m => m.Map<List<TeacherQuizResponseDto>>(quizzes)).Returns(expected);

        // Act
        List<TeacherQuizResponseDto> result = await _useCase.Execute(new QuizQueryParametersDto());

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
        _mockQuizRepo.Setup(r => r.GetByTeacherIdAsync(teacherId, It.IsAny<QuizQueryParametersDto>())).ReturnsAsync([]);
        _mockMapper.Setup(m => m.Map<List<TeacherQuizResponseDto>>(It.IsAny<List<Quiz>>())).Returns([]);

        // Act
        await _useCase.Execute(new QuizQueryParametersDto { Origin = QuizOrigin.ManuallyCreated });

        // Assert
        _mockQuizRepo.Verify(r => r.GetByTeacherIdAsync(
            teacherId,
            It.Is<QuizQueryParametersDto>(q => q.Origin == QuizOrigin.ManuallyCreated)), Times.Once);
    }

    [Fact]
    public async Task Execute_DefaultQuery_PassesDefaultPaginationToRepository()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        _mockCurrentUser.Setup(c => c.UserId).Returns(teacherId.ToString());
        _mockQuizRepo.Setup(r => r.GetByTeacherIdAsync(teacherId, It.IsAny<QuizQueryParametersDto>())).ReturnsAsync([]);
        _mockMapper.Setup(m => m.Map<List<TeacherQuizResponseDto>>(It.IsAny<List<Quiz>>())).Returns([]);

        // Act
        await _useCase.Execute(new QuizQueryParametersDto());

        // Assert
        _mockQuizRepo.Verify(r => r.GetByTeacherIdAsync(
            teacherId,
            It.Is<QuizQueryParametersDto>(q => q.Page == 1 && q.PageSize == 6 && q.Search == null)), Times.Once);
    }

    [Fact]
    public async Task Execute_WithPaginationAndSearch_PassesQueryToRepository()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        _mockCurrentUser.Setup(c => c.UserId).Returns(teacherId.ToString());
        _mockQuizRepo.Setup(r => r.GetByTeacherIdAsync(teacherId, It.IsAny<QuizQueryParametersDto>())).ReturnsAsync([]);
        _mockMapper.Setup(m => m.Map<List<TeacherQuizResponseDto>>(It.IsAny<List<Quiz>>())).Returns([]);
        var query = new QuizQueryParametersDto { Page = 3, PageSize = 10, Search = "math" };

        // Act
        await _useCase.Execute(query);

        // Assert
        _mockQuizRepo.Verify(r => r.GetByTeacherIdAsync(
            teacherId,
            It.Is<QuizQueryParametersDto>(q => q.Page == 3 && q.PageSize == 10 && q.Search == "math")), Times.Once);
    }

    [Fact]
    public async Task Execute_PageLessThanOne_ThrowsValidationException()
    {
        // Arrange
        _mockCurrentUser.Setup(c => c.UserId).Returns(Guid.NewGuid().ToString());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(new QuizQueryParametersDto { Page = 0 }));
        _mockQuizRepo.Verify(r => r.GetByTeacherIdAsync(It.IsAny<Guid>(), It.IsAny<QuizQueryParametersDto>()), Times.Never);
    }

    [Fact]
    public async Task Execute_PageSizeLessThanOne_ThrowsValidationException()
    {
        // Arrange
        _mockCurrentUser.Setup(c => c.UserId).Returns(Guid.NewGuid().ToString());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(new QuizQueryParametersDto { PageSize = 0 }));
        _mockQuizRepo.Verify(r => r.GetByTeacherIdAsync(It.IsAny<Guid>(), It.IsAny<QuizQueryParametersDto>()), Times.Never);
    }

    [Fact]
    public async Task Execute_UserIdInvalid_ThrowsFormatException()
    {
        // Arrange
        _mockCurrentUser.Setup(c => c.UserId).Returns("not-a-guid");

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _useCase.Execute(new QuizQueryParametersDto()));
        _mockQuizRepo.Verify(r => r.GetByTeacherIdAsync(It.IsAny<Guid>(), It.IsAny<QuizQueryParametersDto>()), Times.Never);
    }
}
