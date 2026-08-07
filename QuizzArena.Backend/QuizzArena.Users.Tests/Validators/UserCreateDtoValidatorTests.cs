using FluentAssertions;
using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Application.Validators;
using QuizzArena.Users.Domain.Enums;

namespace QuizzArena.Users.Tests.Validators;

public class UserCreateDtoValidatorTests
{
    private readonly UserCreateDtoValidator _validator = new();

    private static CreateUserDto BuildValidDto() => new()
    {
        Id = Guid.NewGuid(),
        UserName = "druize",
        FirstName = "David",
        LastName = "Ruiz",
        Email = "david.ruiz@jala.university",
        ExternalProvider = "keycloak",
        Role = UserRole.Teacher,
        ProviderId = Guid.NewGuid().ToString(),
        AvatarUrl = "https://cdn.quizzarena.com/avatars/druize.png"
    };

    [Fact]
    public void Validate_WithValidDto_IsValid()
    {
        var result = _validator.Validate(BuildValidDto());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyUserName_IsInvalid(string userName)
    {
        var dto = BuildValidDto();
        dto.UserName = userName;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "UserName is required");
    }

    [Fact]
    public void Validate_WithEmptyFirstName_IsInvalid()
    {
        var dto = BuildValidDto();
        dto.FirstName = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "FirstName is required");
    }

    [Fact]
    public void Validate_WithEmptyLastName_IsInvalid()
    {
        var dto = BuildValidDto();
        dto.LastName = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "LastName is required");
    }

    [Fact]
    public void Validate_WithEmptyEmail_IsInvalid()
    {
        var dto = BuildValidDto();
        dto.Email = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email is required");
    }

    // FluentValidation's EmailAddress() is deliberately lenient (ASP.NET-compatible mode):
    // it only requires a non-empty local part and a single '@'. "missing@domain" is accepted.
    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@jala.university")]
    [InlineData("two@at@signs.com")]
    public void Validate_WithMalformedEmail_IsInvalid(string email)
    {
        var dto = BuildValidDto();
        dto.Email = email;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "A valid email is required");
    }

    [Fact]
    public void Validate_WithEmptyExternalProvider_IsInvalid()
    {
        var dto = BuildValidDto();
        dto.ExternalProvider = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "ExternalProvider is required");
    }

    [Fact]
    public void Validate_WithRoleOutsideEnum_IsInvalid()
    {
        var dto = BuildValidDto();
        dto.Role = (UserRole)99;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Role is invalid");
    }

    [Theory]
    [InlineData(UserRole.Teacher)]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Admin)]
    public void Validate_WithEveryDefinedRole_IsValid(UserRole role)
    {
        var dto = BuildValidDto();
        dto.Role = role;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyProviderId_IsInvalid()
    {
        var dto = BuildValidDto();
        dto.ProviderId = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "ProviderId is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithBlankAvatarUrl_IsValid(string? avatarUrl)
    {
        // AvatarUrl is optional — only a non-blank value has to be a well-formed absolute URL.
        var dto = BuildValidDto();
        dto.AvatarUrl = avatarUrl;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("/relative/path.png")]
    public void Validate_WithMalformedAvatarUrl_IsInvalid(string avatarUrl)
    {
        var dto = BuildValidDto();
        dto.AvatarUrl = avatarUrl;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "AvatarUrl must be a valid URL");
    }

    [Fact]
    public void Validate_WithEmptyDto_ReportsEveryRequiredField()
    {
        var result = _validator.Validate(new CreateUserDto());

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.ErrorMessage).Should().Contain(
        [
            "UserName is required",
            "FirstName is required",
            "LastName is required",
            "Email is required",
            "ExternalProvider is required",
            "ProviderId is required"
        ]);
    }
}
