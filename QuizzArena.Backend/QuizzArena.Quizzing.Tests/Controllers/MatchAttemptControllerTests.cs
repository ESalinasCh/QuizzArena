using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

namespace QuizzArena.Quizzing.Tests.Controllers;

public class MatchAttemptControllerTests
{
    private readonly Mock<IStartAttemptUseCase> _mockStartAttemptUseCase;
    private readonly Mock<ISubmitAnswersUseCase> _mockSubmitAnswersUseCase;
    private readonly Mock<ITrackAnswerUseCase> _mockTrackAnswerUseCase;
    private readonly Mock<IFinishMatchTrackedUseCase> _mockFinishMatchTrackedUseCase;
    private readonly Mock<IGetMatchAttemptsByStudent> _mockGetMatchAttemptsByStudent;
    private readonly Mock<IGetMatchAttemptDetail> _mockGetMatchAttemptDetail;
    private readonly Mock<IGetMatchAttemptGradesUseCase> _mockGetMatchAttemptGradesUseCase;
    private readonly Mock<IResetMatchAttemptUseCase> _mockResetMatchAttemptUseCase;
    private readonly MatchAttemptController _controller;

    public MatchAttemptControllerTests()
    {
        _mockStartAttemptUseCase = new Mock<IStartAttemptUseCase>();
        _mockSubmitAnswersUseCase = new Mock<ISubmitAnswersUseCase>();
        _mockTrackAnswerUseCase = new Mock<ITrackAnswerUseCase>();
        _mockFinishMatchTrackedUseCase = new Mock<IFinishMatchTrackedUseCase>();
        _mockGetMatchAttemptsByStudent = new Mock<IGetMatchAttemptsByStudent>();
        _mockGetMatchAttemptDetail = new Mock<IGetMatchAttemptDetail>();
        _mockGetMatchAttemptGradesUseCase = new Mock<IGetMatchAttemptGradesUseCase>();
        _mockResetMatchAttemptUseCase = new Mock<IResetMatchAttemptUseCase>();

        _controller = new MatchAttemptController(
            _mockStartAttemptUseCase.Object,
            _mockSubmitAnswersUseCase.Object,
            _mockTrackAnswerUseCase.Object,
            _mockFinishMatchTrackedUseCase.Object,
            _mockGetMatchAttemptsByStudent.Object,
            _mockGetMatchAttemptDetail.Object,
            _mockGetMatchAttemptGradesUseCase.Object,
            _mockResetMatchAttemptUseCase.Object
        );
    }

    [Fact]
    public async Task StartMatchAttemp_ReturnsOkWithAttemptAndQuestions()
    {
        // Arrange
        var dto = new StartAttemptRequestDto { MatchId = Guid.NewGuid() };
        var expected = new StartAttemptResponseDto
        {
            MatchId = dto.MatchId,
            MatchAttemptId = Guid.NewGuid(),
            AnsweredQuestions = 0,
            TotalQuestions = 1,
            Questions =
            [
                new StartAttemptQuestionResponseDto
                {
                    Id = Guid.NewGuid(),
                    Statement = "What is the SI unit of force?",
                    QuestionType = QuestionType.SingleChoice,
                    Options = [new StartAttemptOptionResponseDto { Id = Guid.NewGuid(), Label = "Newton" }]
                }
            ]
        };

        _mockStartAttemptUseCase.Setup(uc => uc.Execute(dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.StartMatchAttemp(dto);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockStartAttemptUseCase.Verify(uc => uc.Execute(dto), Times.Once);
    }

    [Fact]
    public async Task StartMatchAttemp_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var dto = new StartAttemptRequestDto { MatchId = Guid.NewGuid() };
        _mockStartAttemptUseCase
            .Setup(uc => uc.Execute(dto))
            .ThrowsAsync(new InvalidOperationException("no attempts left"));

        // Act
        Func<Task> act = () => _controller.StartMatchAttemp(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("no attempts left");
    }

    [Fact]
    public async Task SubmitAnswers_ReturnsOkWithResults()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var dto = new SubmitAnswersRequestDto
        {
            Answers = [new SubmitAnswerBody(questionId, [optionId], DateTimeOffset.UtcNow)]
        };
        var expected = new SubmitAnswersResponseDto
        {
            AttemptId = attemptId,
            ScorePercentage = 100,
            CorrectCount = 1,
            IncorrectCount = 0,
            TotalQuestions = 1,
            Questions = [new QuestionResultDto(questionId, "Question", [optionId], [optionId], true)]
        };

        _mockSubmitAnswersUseCase.Setup(uc => uc.Execute(attemptId, dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.SubmitAnswers(attemptId, dto);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockSubmitAnswersUseCase.Verify(uc => uc.Execute(attemptId, dto), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswers_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var dto = new SubmitAnswersRequestDto { Answers = [] };
        _mockSubmitAnswersUseCase
            .Setup(uc => uc.Execute(attemptId, dto))
            .ThrowsAsync(new InvalidOperationException("attempt already completed"));

        // Act
        Func<Task> act = () => _controller.SubmitAnswers(attemptId, dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("attempt already completed");
    }

    [Fact]
    public async Task TrackAnswer_ReturnsOkWithProgress()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var dto = new TrackAnswerRequestDto { SelectedOptionIds = [Guid.NewGuid()] };
        var expected = new MatchAttemptSmallProgressDto { AnsweredQuestions = 3, TotalQuestions = 10 };

        _mockTrackAnswerUseCase.Setup(uc => uc.Execute(attemptId, questionId, dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.TrackAnswer(attemptId, questionId, dto);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockTrackAnswerUseCase.Verify(uc => uc.Execute(attemptId, questionId, dto), Times.Once);
    }

    [Fact]
    public async Task TrackAnswer_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var dto = new TrackAnswerRequestDto();
        _mockTrackAnswerUseCase
            .Setup(uc => uc.Execute(attemptId, questionId, dto))
            .ThrowsAsync(new KeyNotFoundException("attempt not found"));

        // Act
        Func<Task> act = () => _controller.TrackAnswer(attemptId, questionId, dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("attempt not found");
    }

    [Fact]
    public async Task CompleteAttempt_ReturnsOkWithFinishedAttempt()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var expected = new FinishedMatchTrackedDto
        {
            AttemptId = attemptId,
            AnsweredQuestions = 10,
            TotalQuestions = 10,
            Answers = [new AnswerTrackedDto { Id = Guid.NewGuid(), Number = 1, Text = "Question 1" }]
        };

        _mockFinishMatchTrackedUseCase.Setup(uc => uc.Execute(attemptId)).ReturnsAsync(expected);

        // Act
        var result = await _controller.CompleteAttempt(attemptId);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockFinishMatchTrackedUseCase.Verify(uc => uc.Execute(attemptId), Times.Once);
    }

    [Fact]
    public async Task CompleteAttempt_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        _mockFinishMatchTrackedUseCase
            .Setup(uc => uc.Execute(attemptId))
            .ThrowsAsync(new InvalidOperationException("attempt already completed"));

        // Act
        Func<Task> act = () => _controller.CompleteAttempt(attemptId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("attempt already completed");
    }

    [Fact]
    public async Task GetMatchAttemptGrades_ReturnsOkWithGrades()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var filters = new MatchAttemptFilters { Status = QuizAttemptStatus.Completed, Page = 1, PageSize = 5 };
        var expected = new List<MatchAttemptGradesResponseDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Nickname = "student1",
                Status = QuizAttemptStatus.Completed,
                Score = 80,
                UserId = Guid.NewGuid(),
                MatchId = matchId
            }
        };

        _mockGetMatchAttemptGradesUseCase.Setup(uc => uc.Execute(matchId, filters)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetMatchAttemptGrades(matchId, filters);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockGetMatchAttemptGradesUseCase.Verify(uc => uc.Execute(matchId, filters), Times.Once);
    }

    [Fact]
    public async Task GetMatchAttemptGrades_WhenNoGrades_ReturnsOkWithEmptyList()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var filters = new MatchAttemptFilters();
        _mockGetMatchAttemptGradesUseCase.Setup(uc => uc.Execute(matchId, filters)).ReturnsAsync([]);

        // Act
        var result = await _controller.GetMatchAttemptGrades(matchId, filters);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<List<MatchAttemptGradesResponseDto>>().Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMatchAttemptGrades_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var filters = new MatchAttemptFilters();
        _mockGetMatchAttemptGradesUseCase
            .Setup(uc => uc.Execute(matchId, filters))
            .ThrowsAsync(new KeyNotFoundException("match not found"));

        // Act
        Func<Task> act = () => _controller.GetMatchAttemptGrades(matchId, filters);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("match not found");
    }

    [Fact]
    public async Task GetMyMatchAttempts_ReturnsOkWithAttempts()
    {
        // Arrange
        var filters = new MatchAttemptFilters { MatchMode = MatchMode.Exam, Page = 1, PageSize = 5 };
        var expected = new List<GetMatchAttemptDTO>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Parcial 1",
                CourseName = "Physics",
                Score = 75,
                Status = QuizAttemptStatus.Completed,
                Duration = 30
            }
        };

        _mockGetMatchAttemptsByStudent.Setup(uc => uc.Execute(filters)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetMyMatchAttempts(filters);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockGetMatchAttemptsByStudent.Verify(uc => uc.Execute(filters), Times.Once);
    }

    [Fact]
    public async Task GetMyMatchAttempts_WhenNoAttempts_ReturnsOkWithEmptyList()
    {
        // Arrange
        var filters = new MatchAttemptFilters();
        _mockGetMatchAttemptsByStudent.Setup(uc => uc.Execute(filters)).ReturnsAsync([]);

        // Act
        var result = await _controller.GetMyMatchAttempts(filters);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<List<GetMatchAttemptDTO>>().Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyMatchAttempts_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var filters = new MatchAttemptFilters();
        _mockGetMatchAttemptsByStudent
            .Setup(uc => uc.Execute(filters))
            .ThrowsAsync(new InvalidOperationException("invalid filters"));

        // Act
        Func<Task> act = () => _controller.GetMyMatchAttempts(filters);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("invalid filters");
    }

    [Fact]
    public async Task GetMatchAttemptDetail_ReturnsOkWithDetail()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var expected = new GetMatchAttemptDetailDTO
        {
            Id = attemptId,
            Score = 90,
            Status = QuizAttemptStatus.Completed,
            Questions =
            [
                new GetMatchAttemptQuestionDTO
                {
                    QuestionId = Guid.NewGuid(),
                    Content = "What is the SI unit of force?",
                    IsCorrect = true,
                    Options = [new GetMatchAttemptOptionDTO { Id = Guid.NewGuid(), Description = "Newton", IsCorrect = true }]
                }
            ]
        };

        _mockGetMatchAttemptDetail.Setup(uc => uc.Execute(attemptId)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetMatchAttemptDetail(attemptId);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockGetMatchAttemptDetail.Verify(uc => uc.Execute(attemptId), Times.Once);
    }

    [Fact]
    public async Task GetMatchAttemptDetail_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        _mockGetMatchAttemptDetail
            .Setup(uc => uc.Execute(attemptId))
            .ThrowsAsync(new KeyNotFoundException("attempt not found"));

        // Act
        Func<Task> act = () => _controller.GetMatchAttemptDetail(attemptId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("attempt not found");
    }

    [Fact]
    public async Task ResetMatchAttempt_ReturnsOkAndInvokesUseCase()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mockResetMatchAttemptUseCase.Setup(uc => uc.Execute(matchId, userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ResetMatchAttempt(matchId, userId);

        // Assert
        result.Result.Should().BeOfType<OkResult>();
        _mockResetMatchAttemptUseCase.Verify(uc => uc.Execute(matchId, userId), Times.Once);
    }

    [Fact]
    public async Task ResetMatchAttempt_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mockResetMatchAttemptUseCase
            .Setup(uc => uc.Execute(matchId, userId))
            .ThrowsAsync(new KeyNotFoundException("attempt not found"));

        // Act
        Func<Task> act = () => _controller.ResetMatchAttempt(matchId, userId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("attempt not found");
    }
}
