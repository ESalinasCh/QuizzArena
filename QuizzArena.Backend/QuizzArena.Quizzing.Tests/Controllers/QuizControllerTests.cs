using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

namespace QuizzArena.Quizzing.Tests.Controllers;

public class QuizControllerTests
{
    private readonly Mock<ICreateExamUseCase> _mockCreateExamUseCase;
    private readonly Mock<IGetTeacherQuizzesUseCase> _mockGetTeacherQuizzesUseCase;
    private readonly QuizController _controller;

    public QuizControllerTests()
    {
        _mockCreateExamUseCase = new Mock<ICreateExamUseCase>();
        _mockGetTeacherQuizzesUseCase = new Mock<IGetTeacherQuizzesUseCase>();

        _controller = new QuizController(
            _mockCreateExamUseCase.Object,
            _mockGetTeacherQuizzesUseCase.Object
        );
    }

    [Fact]
    public async Task CreateExam_ReturnsOkWithCreatedQuiz()
    {
        // Arrange
        var dto = new CreateExamDto
        {
            Title = "Final exam",
            Description = "Covers every unit",
            QuestionIds = [Guid.NewGuid(), Guid.NewGuid()]
        };
        var expected = new CreateQuizResponseDto
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Status = QuizStatus.draft
        };

        _mockCreateExamUseCase.Setup(uc => uc.Execute(dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.CreateExam(dto);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockCreateExamUseCase.Verify(uc => uc.Execute(dto), Times.Once);
    }

    [Fact]
    public async Task CreateExam_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var dto = new CreateExamDto();
        _mockCreateExamUseCase
            .Setup(uc => uc.Execute(dto))
            .ThrowsAsync(new InvalidOperationException("at least one question is required"));

        // Act
        Func<Task> act = () => _controller.CreateExam(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("at least one question is required");
    }

    [Fact]
    public async Task GetTeacherQuizzes_ReturnsOkWithQuizzes()
    {
        // Arrange
        var query = new QuizQueryParametersDto
        {
            Origin = QuizOrigin.ManuallyCreated,
            Status = QuizStatus.published,
            Page = 1,
            PageSize = 5
        };
        var expected = new List<TeacherQuizResponseDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Unit 1",
                Description = "Kinematics",
                Status = QuizStatus.published,
                Origin = QuizOrigin.ManuallyCreated
            }
        };

        _mockGetTeacherQuizzesUseCase.Setup(uc => uc.Execute(query)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetTeacherQuizzes(query);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockGetTeacherQuizzesUseCase.Verify(uc => uc.Execute(query), Times.Once);
    }

    [Fact]
    public async Task GetTeacherQuizzes_WhenNoQuizzes_ReturnsOkWithEmptyList()
    {
        // Arrange
        var query = new QuizQueryParametersDto();
        _mockGetTeacherQuizzesUseCase.Setup(uc => uc.Execute(query)).ReturnsAsync([]);

        // Act
        var result = await _controller.GetTeacherQuizzes(query);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<List<TeacherQuizResponseDto>>().Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTeacherQuizzes_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var query = new QuizQueryParametersDto();
        _mockGetTeacherQuizzesUseCase
            .Setup(uc => uc.Execute(query))
            .ThrowsAsync(new InvalidOperationException("invalid query"));

        // Act
        Func<Task> act = () => _controller.GetTeacherQuizzes(query);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("invalid query");
    }
}
