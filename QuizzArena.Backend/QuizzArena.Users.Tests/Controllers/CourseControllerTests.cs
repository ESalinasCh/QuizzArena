using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizzArena.Users.Application.DTOs.Course;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Infrastructure.Adapters.In.Web;

namespace QuizzArena.Users.Tests.Controllers;

public class CourseControllerTests
{
    private readonly Mock<IGetTeacherCoursesUseCase> _mockGetTeacherCoursesUseCase;
    private readonly CourseController _controller;

    public CourseControllerTests()
    {
        _mockGetTeacherCoursesUseCase = new Mock<IGetTeacherCoursesUseCase>();
        _controller = new CourseController(_mockGetTeacherCoursesUseCase.Object);
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
    public async Task GetTeacherCourses_WithValidSubClaim_ReturnsOkWithList()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var expected = new List<CourseDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Physics", Description = "Mechanics" }
        };

        SetUserClaim(teacherId.ToString());
        _mockGetTeacherCoursesUseCase.Setup(uc => uc.Execute(teacherId)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetTeacherCourses();

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetTeacherCourses_WithMissingSubClaim_ReturnsUnauthorized()
    {
        // Arrange
        SetUserClaim(null);

        // Act
        var result = await _controller.GetTeacherCourses();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
        _mockGetTeacherCoursesUseCase.Verify(uc => uc.Execute(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetTeacherCourses_WithInvalidSubClaim_ReturnsUnauthorized()
    {
        // Arrange
        SetUserClaim("not-a-guid");

        // Act
        var result = await _controller.GetTeacherCourses();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
        _mockGetTeacherCoursesUseCase.Verify(uc => uc.Execute(It.IsAny<Guid>()), Times.Never);
    }
}
