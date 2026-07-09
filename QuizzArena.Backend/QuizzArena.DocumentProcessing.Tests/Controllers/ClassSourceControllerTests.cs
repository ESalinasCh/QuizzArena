using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;

namespace QuizzArena.DocumentProcessing.Tests.Controllers;

public class ClassSourceControllerTests
{
    private readonly Mock<IUploadSourceUseCase> _mockUploadUseCase;
    private readonly Mock<IGetClassSourcesUseCase> _mockGetClassSourcesUseCase;
    private readonly ClassSourceController _controller;

    public ClassSourceControllerTests()
    {
        _mockUploadUseCase = new Mock<IUploadSourceUseCase>();
        _mockGetClassSourcesUseCase = new Mock<IGetClassSourcesUseCase>();
        _controller = new ClassSourceController(_mockUploadUseCase.Object, _mockGetClassSourcesUseCase.Object);
    }

    private void SetUserClaim(string? subValue)
    {
        var claims = subValue is null
            ? new List<Claim>()
            : new List<Claim> { new Claim("sub", subValue) };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetMyClassSources_WithValidSubClaim_ReturnsOkWithList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expected = new List<GetClassSourceResponseDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Clase 1", Status = SourceStatus.Completed, CourseId = Guid.NewGuid(), ProcessingJobsIds = [] }
        };

        SetUserClaim(userId.ToString());
        _mockGetClassSourcesUseCase.Setup(uc => uc.Execute(userId)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetMyClassSources();

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetMyClassSources_WithMissingSubClaim_ReturnsUnauthorized()
    {
        // Arrange
        SetUserClaim(null);

        // Act
        var result = await _controller.GetMyClassSources();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
        _mockGetClassSourcesUseCase.Verify(uc => uc.Execute(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetMyClassSources_WithInvalidSubClaim_ReturnsUnauthorized()
    {
        // Arrange
        SetUserClaim("not-a-guid");

        // Act
        var result = await _controller.GetMyClassSources();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
        _mockGetClassSourcesUseCase.Verify(uc => uc.Execute(It.IsAny<Guid>()), Times.Never);
    }
}
