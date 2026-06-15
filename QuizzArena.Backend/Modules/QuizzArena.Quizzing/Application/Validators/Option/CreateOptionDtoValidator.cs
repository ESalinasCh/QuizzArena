using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.Validators.Option;

public class CreateOptionDtoValidator : AbstractValidator<CreateOptionDto>
{
    public CreateOptionDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");

        RuleFor(x => x.IsCorrect)
            .NotNull()
            .WithMessage("IsCorrect is required");

        RuleFor(x => x.Position)
            .NotEmpty()
            .WithMessage("Position is required")
            .GreaterThan(0)
            .WithMessage("Position must be greater than 0");
    }
}
