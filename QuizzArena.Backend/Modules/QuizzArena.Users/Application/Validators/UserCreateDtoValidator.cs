using FluentValidation;
using QuizzArena.Users.Application.DTOs.User;

namespace QuizzArena.Users.Application.Validators;

internal class UserCreateDtoValidator : AbstractValidator<CreateUserDto>
{
    public UserCreateDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("UserName is required");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("FirstName is required");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("LastName is required");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("A valid email is required");

        RuleFor(x => x.ExternalProvider)
            .NotEmpty()
            .WithMessage("ExternalProvider is required");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Role is invalid");

        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .WithMessage("ProviderId is required");

        RuleFor(x => x.AvatarUrl)
            .Must(uri => string.IsNullOrWhiteSpace(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("AvatarUrl must be a valid URL");
    }
}
