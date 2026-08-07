using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;
using Shared.Contracts.DTOs;

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

    private static UploadClassSourceRequestDto BuildUploadRequest(string fileName = "clase-1.mp4")
    {
        var content = new MemoryStream("fake content"u8.ToArray());
        var file = new FormFile(content, 0, content.Length, "File", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "video/mp4"
        };

        return new UploadClassSourceRequestDto
        {
            Name = "Clase 1",
            CourseId = Guid.NewGuid(),
            File = file
        };
    }

    [Fact]
    public async Task UploadClassSource_ReturnsOkWithUploadedSource()
    {
        var dto = BuildUploadRequest();
        var expected = new UploadClassSourceResponseDto
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CourseId = dto.CourseId,
            UserId = Guid.NewGuid(),
            SourceType = SourceType.Video,
            Status = SourceStatus.Pending
        };

        _mockUploadUseCase.Setup(uc => uc.Execute(dto)).ReturnsAsync(expected);

        var result = await _controller.UploadClassSource(dto);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
        _mockUploadUseCase.Verify(uc => uc.Execute(dto), Times.Once);
    }

    [Fact]
    public async Task UploadClassSource_WhenUseCaseThrows_PropagatesException()
    {
        var dto = BuildUploadRequest("clase-1.exe");
        _mockUploadUseCase
            .Setup(uc => uc.Execute(dto))
            .ThrowsAsync(new InvalidOperationException("unsupported file type"));

        Func<Task> act = () => _controller.UploadClassSource(dto);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("unsupported file type");
    }

    [Fact]
    public async Task GetMyClassSources_WithValidSubClaim_ReturnsOkWithList()
    {
        var userId = Guid.NewGuid();
        var query = new PagedRequest();
        var expected = new List<GetClassSourceResponseDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Clase 1", Status = SourceStatus.Completed, CourseId = Guid.NewGuid(), ProcessingJobsIds = [] }
        };

        SetUserClaim(userId.ToString());
        _mockGetClassSourcesUseCase
            .Setup(uc => uc.Execute(userId, It.IsAny<PagedRequest>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyClassSources(query);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetMyClassSources_WithMissingSubClaim_ReturnsUnauthorized()
    {
        SetUserClaim(null);

        var result = await _controller.GetMyClassSources(new PagedRequest());

        result.Result.Should().BeOfType<UnauthorizedResult>();
        _mockGetClassSourcesUseCase.Verify(uc => uc.Execute(It.IsAny<Guid>(), It.IsAny<PagedRequest>()), Times.Never);
    }

    [Fact]
    public async Task GetMyClassSources_WithInvalidSubClaim_ReturnsUnauthorized()
    {
        SetUserClaim("not-a-guid");

        var result = await _controller.GetMyClassSources(new PagedRequest());

        result.Result.Should().BeOfType<UnauthorizedResult>();
        _mockGetClassSourcesUseCase.Verify(uc => uc.Execute(It.IsAny<Guid>(), It.IsAny<PagedRequest>()), Times.Never);
    }
}
