using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

namespace QuizzArena.Quizzing.Tests.Controllers;

public class QuestionControllerTests
{
    private readonly Mock<IGetQuestionsUseCase> _mockGetQuestionsUseCase;
    private readonly Mock<ICreateManualQuestionUseCase> _mockCreateManualQuestionUseCase;
    private readonly Mock<IUpdateQuestionUseCase> _mockUpdateQuestionUseCase;
    private readonly Mock<IDeleteQuestionUseCase> _mockDeleteQuestionUseCase;
    private readonly QuestionController _controller;

    public QuestionControllerTests()
    {
        _mockGetQuestionsUseCase = new Mock<IGetQuestionsUseCase>();
        _mockCreateManualQuestionUseCase = new Mock<ICreateManualQuestionUseCase>();
        _mockUpdateQuestionUseCase = new Mock<IUpdateQuestionUseCase>();
        _mockDeleteQuestionUseCase = new Mock<IDeleteQuestionUseCase>();

        _controller = new QuestionController(
            _mockGetQuestionsUseCase.Object,
            _mockCreateManualQuestionUseCase.Object,
            _mockUpdateQuestionUseCase.Object,
            _mockDeleteQuestionUseCase.Object
        );
    }

    private static ResponseQuestionDto BuildQuestion(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Content = "What is the SI unit of force?",
        Justification = "Newton is the SI unit of force.",
        Status = QuestionStatus.Draft,
        Type = QuestionType.SingleChoice,
        Options =
        [
            new ResponseOptionDto { Id = Guid.NewGuid(), Description = "Newton", IsCorrect = true, Position = 1 },
            new ResponseOptionDto { Id = Guid.NewGuid(), Description = "Joule", IsCorrect = false, Position = 2 }
        ]
    };

    [Fact]
    public async Task GetQuestions_ReturnsOkWithQuestions()
    {
        // Arrange
        var filters = new QuestionFilters
        {
            Status = QuestionStatus.Verified,
            ProcessingJobIds = [Guid.NewGuid()],
            Page = 1,
            PageSize = 5
        };
        var expected = new List<ResponseQuestionDto> { BuildQuestion() };

        _mockGetQuestionsUseCase.Setup(uc => uc.Execute(filters)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetQuestions(filters);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockGetQuestionsUseCase.Verify(uc => uc.Execute(filters), Times.Once);
    }

    [Fact]
    public async Task GetQuestions_FiltersByQuestionIds_PassesFiltersThrough()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var filters = new QuestionFilters { QuestionIds = [questionId] };
        var expected = new List<ResponseQuestionDto> { BuildQuestion(questionId) };

        _mockGetQuestionsUseCase
            .Setup(uc => uc.Execute(It.Is<QuestionFilters>(f => f.QuestionIds.Contains(questionId))))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetQuestions(filters);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetQuestions_WhenNoQuestions_ReturnsOkWithEmptyList()
    {
        // Arrange
        var filters = new QuestionFilters();
        _mockGetQuestionsUseCase.Setup(uc => uc.Execute(filters)).ReturnsAsync([]);

        // Act
        var result = await _controller.GetQuestions(filters);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<List<ResponseQuestionDto>>().Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetQuestions_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var filters = new QuestionFilters();
        _mockGetQuestionsUseCase
            .Setup(uc => uc.Execute(filters))
            .ThrowsAsync(new InvalidOperationException("invalid filters"));

        // Act
        Func<Task> act = () => _controller.GetQuestions(filters);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("invalid filters");
    }

    [Fact]
    public async Task CreateQuestion_ReturnsOkWithCreatedQuestion()
    {
        // Arrange
        var dto = new CreateManualQuestionDto
        {
            Content = "What is the SI unit of force?",
            Justification = "Newton is the SI unit of force.",
            Type = QuestionType.SingleChoice,
            Options =
            [
                new CreateOptionDto { Description = "Newton", IsCorrect = true, Position = 1 },
                new CreateOptionDto { Description = "Joule", IsCorrect = false, Position = 2 }
            ]
        };
        var expected = BuildQuestion();

        _mockCreateManualQuestionUseCase.Setup(uc => uc.Execute(dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.CreateQuestion(dto);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockCreateManualQuestionUseCase.Verify(uc => uc.Execute(dto), Times.Once);
    }

    [Fact]
    public async Task CreateQuestion_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var dto = new CreateManualQuestionDto();
        _mockCreateManualQuestionUseCase
            .Setup(uc => uc.Execute(dto))
            .ThrowsAsync(new InvalidOperationException("a question needs at least two options"));

        // Act
        Func<Task> act = () => _controller.CreateQuestion(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("a question needs at least two options");
    }

    [Fact]
    public async Task UpdateQuestion_ReturnsOkWithUpdatedQuestion()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var dto = new UpdateQuestionDto
        {
            QuestionId = questionId,
            Content = "Updated content",
            Status = QuestionStatus.Verified,
            Options = [new UpdateOptionDto { OptionId = Guid.NewGuid(), Description = "Newton", IsCorrect = true }]
        };
        var expected = BuildQuestion(questionId);

        _mockUpdateQuestionUseCase.Setup(uc => uc.Execute(dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.UpdateQuestion(dto);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockUpdateQuestionUseCase.Verify(uc => uc.Execute(dto), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestion_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var dto = new UpdateQuestionDto { QuestionId = Guid.NewGuid() };
        _mockUpdateQuestionUseCase
            .Setup(uc => uc.Execute(dto))
            .ThrowsAsync(new KeyNotFoundException("question not found"));

        // Act
        Func<Task> act = () => _controller.UpdateQuestion(dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("question not found");
    }

    [Fact]
    public async Task DeleteQuestion_ReturnsOkWithDeletedQuestion()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var expected = BuildQuestion(questionId);

        _mockDeleteQuestionUseCase.Setup(uc => uc.Execute(questionId)).ReturnsAsync(expected);

        // Act
        var result = await _controller.DeleteQuestion(questionId);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockDeleteQuestionUseCase.Verify(uc => uc.Execute(questionId), Times.Once);
    }

    [Fact]
    public async Task DeleteQuestion_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        _mockDeleteQuestionUseCase
            .Setup(uc => uc.Execute(questionId))
            .ThrowsAsync(new KeyNotFoundException("question not found"));

        // Act
        Func<Task> act = () => _controller.DeleteQuestion(questionId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("question not found");
    }
}
