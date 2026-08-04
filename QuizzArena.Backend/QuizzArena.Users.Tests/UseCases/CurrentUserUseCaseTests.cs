using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using QuizzArena.Users.Application.UseCases.User;

namespace QuizzArena.Users.Tests.UseCases;

public class CurrentUserUseCaseTests
{
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly CurrentUserUseCase _useCase;

    public CurrentUserUseCaseTests()
    {
        _mockAccessor = new Mock<IHttpContextAccessor>();
        _useCase = new CurrentUserUseCase(_mockAccessor.Object);
    }

    private void SetHttpContext(params Claim[] claims)
    {
        // GetRole() resolves through IsInRole, so the identity needs an explicit role claim type.
        var identity = new ClaimsIdentity(claims, "test", "name", System.Security.Claims.ClaimTypes.Role);
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        _mockAccessor.Setup(a => a.HttpContext).Returns(context);
    }

    [Fact]
    public void UserId_WithSubClaim_ReturnsSubValue()
    {
        var userId = Guid.NewGuid().ToString();
        SetHttpContext(new Claim("sub", userId));

        _useCase.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserName_WithPreferredUsernameClaim_ReturnsValue()
    {
        SetHttpContext(new Claim("preferred_username", "druize"));

        _useCase.UserName.Should().Be("druize");
    }

    [Fact]
    public void FullName_WithNameClaim_ReturnsValue()
    {
        SetHttpContext(new Claim("name", "David Ruiz"));

        _useCase.FullName.Should().Be("David Ruiz");
    }

    [Theory]
    [InlineData("teacher", "Teacher")]
    [InlineData("student", "Student")]
    [InlineData("admin", "Admin")]
    public void Role_WithRoleClaim_ReturnsRoleName(string claimValue, string expected)
    {
        SetHttpContext(new Claim(System.Security.Claims.ClaimTypes.Role, claimValue));

        _useCase.Role.Should().Be(expected);
    }

    [Fact]
    public void Role_WithoutAnyKnownRole_Throws()
    {
        SetHttpContext(new Claim(System.Security.Claims.ClaimTypes.Role, "guest"));

        Action act = () => _ = _useCase.Role;

        act.Should().Throw<InvalidOperationException>().WithMessage("No valid role found");
    }

    [Fact]
    public void UserId_WithMissingClaim_ReturnsEmptyString()
    {
        SetHttpContext();

        _useCase.UserId.Should().BeEmpty();
    }

    [Fact]
    public void UserId_WithNoHttpContext_ThrowsUnauthorized()
    {
        _mockAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        Action act = () => _ = _useCase.UserId;

        act.Should().Throw<UnauthorizedAccessException>();
    }
}
