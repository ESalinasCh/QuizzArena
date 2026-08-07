using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.In.Match;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Tests.Controllers;

public class MatchControllerTests
{
    private readonly Mock<IGetMatchesUseCase> _mockGetMatchesUseCase;
    private readonly Mock<ICreateMatchUseCase> _mockCreateMatchUseCase;
    private readonly Mock<IPublishMatchUseCase> _mockPublishMatchUseCase;
    private readonly Mock<IUnpublishMatchUseCase> _mockUnpublishMatchUseCase;
    private readonly Mock<IGetMatchByIdUseCase> _mockGetMatchByIdUseCase;
    private readonly Mock<IUpdateMatchUseCase> _mockUpdateMatchUseCase;
    private readonly MatchController _controller;

    public MatchControllerTests()
    {
        _mockGetMatchesUseCase = new Mock<IGetMatchesUseCase>();
        _mockCreateMatchUseCase = new Mock<ICreateMatchUseCase>();
        _mockPublishMatchUseCase = new Mock<IPublishMatchUseCase>();
        _mockUnpublishMatchUseCase = new Mock<IUnpublishMatchUseCase>();
        _mockGetMatchByIdUseCase = new Mock<IGetMatchByIdUseCase>();
        _mockUpdateMatchUseCase = new Mock<IUpdateMatchUseCase>();

        _controller = new MatchController(
            _mockGetMatchesUseCase.Object,
            _mockCreateMatchUseCase.Object,
            _mockPublishMatchUseCase.Object,
            _mockUnpublishMatchUseCase.Object,
            _mockGetMatchByIdUseCase.Object,
            _mockUpdateMatchUseCase.Object
        );
    }

    [Fact]
    public async Task GetMatches_ReturnsOkWithMatches()
    {
        // Arrange
        var query = new MatchQueryParametersDto { Status = MatchStatus.Active, Page = 1, PageSize = 5 };
        var expected = new List<MatchResponseDto>
        {
            new() { Id = Guid.NewGuid(), QuizId = Guid.NewGuid(), Title = "Parcial 1", CourseName = "Physics" }
        };

        _mockGetMatchesUseCase.Setup(uc => uc.Execute(query)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetMatches(query);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockGetMatchesUseCase.Verify(uc => uc.Execute(query), Times.Once);
    }

    [Fact]
    public async Task GetMatches_WhenNoMatches_ReturnsOkWithEmptyList()
    {
        // Arrange
        var query = new MatchQueryParametersDto();
        _mockGetMatchesUseCase.Setup(uc => uc.Execute(query)).ReturnsAsync([]);

        // Act
        var result = await _controller.GetMatches(query);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<List<MatchResponseDto>>().Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMatches_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var query = new MatchQueryParametersDto();
        _mockGetMatchesUseCase
            .Setup(uc => uc.Execute(query))
            .ThrowsAsync(new InvalidOperationException("invalid query"));

        // Act
        Func<Task> act = () => _controller.GetMatches(query);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("invalid query");
    }

    [Fact]
    public async Task CreateMatch_ReturnsOkWithCreatedMatch()
    {
        // Arrange
        var dto = new MatchCreateDto
        {
            QuizId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            TimeMinutes = 30,
            AttemptsAmount = 2
        };
        var expected = new MatchCreatedResponseDto
        {
            Id = Guid.NewGuid(),
            QuizId = dto.QuizId,
            CourseId = dto.CourseId,
            Status = MatchStatus.Pending
        };

        _mockCreateMatchUseCase.Setup(uc => uc.Execute(dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.CreateMatch(dto);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockCreateMatchUseCase.Verify(uc => uc.Execute(dto), Times.Once);
    }

    [Fact]
    public async Task CreateMatch_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var dto = new MatchCreateDto();
        _mockCreateMatchUseCase
            .Setup(uc => uc.Execute(dto))
            .ThrowsAsync(new InvalidOperationException("quiz not found"));

        // Act
        Func<Task> act = () => _controller.CreateMatch(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("quiz not found");
    }

    [Fact]
    public async Task PublishMatch_ReturnsOkWithPublicationResponse()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var expected = new MatchPublicationResponseDto
        {
            Id = matchId,
            PublicationStatus = MatchStatus.Active,
            ShareCode = "ABC123"
        };

        _mockPublishMatchUseCase.Setup(uc => uc.Execute(matchId)).ReturnsAsync(expected);

        // Act
        var result = await _controller.PublishMatch(matchId);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockPublishMatchUseCase.Verify(uc => uc.Execute(matchId), Times.Once);
    }

    [Fact]
    public async Task PublishMatch_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        _mockPublishMatchUseCase
            .Setup(uc => uc.Execute(matchId))
            .ThrowsAsync(new KeyNotFoundException("match not found"));

        // Act
        Func<Task> act = () => _controller.PublishMatch(matchId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("match not found");
    }

    [Fact]
    public async Task UnpublishMatch_ReturnsOkWithPublicationResponse()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var expected = new MatchPublicationResponseDto
        {
            Id = matchId,
            PublicationStatus = MatchStatus.Pending,
            ShareCode = ""
        };

        _mockUnpublishMatchUseCase.Setup(uc => uc.Execute(matchId)).ReturnsAsync(expected);

        // Act
        var result = await _controller.UnpublishMatch(matchId);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockUnpublishMatchUseCase.Verify(uc => uc.Execute(matchId), Times.Once);
    }

    [Fact]
    public async Task UnpublishMatch_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        _mockUnpublishMatchUseCase
            .Setup(uc => uc.Execute(matchId))
            .ThrowsAsync(new KeyNotFoundException("match not found"));

        // Act
        Func<Task> act = () => _controller.UnpublishMatch(matchId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("match not found");
    }

    [Fact]
    public async Task UpdateMatch_ReturnsOkWithUpdatedMatch()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var dto = new MatchUpdateDto { Title = "Parcial 2", TimeMinutes = 45 };
        var expected = new MatchUpdatedResponseDto
        {
            Id = matchId,
            Title = dto.Title,
            TimeMinutes = 45
        };

        _mockUpdateMatchUseCase.Setup(uc => uc.Execute(matchId, dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.UpdateMatch(matchId, dto);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockUpdateMatchUseCase.Verify(uc => uc.Execute(matchId, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateMatch_WhenUseCaseThrows_PropagatesException()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var dto = new MatchUpdateDto();
        _mockUpdateMatchUseCase
            .Setup(uc => uc.Execute(matchId, dto))
            .ThrowsAsync(new InvalidOperationException("match already started"));

        // Act
        Func<Task> act = () => _controller.UpdateMatch(matchId, dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("match already started");
    }
}
