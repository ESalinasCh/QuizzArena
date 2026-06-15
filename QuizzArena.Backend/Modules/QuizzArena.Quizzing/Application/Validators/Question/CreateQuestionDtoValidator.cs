
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.Validators.Question;

public class CreateQuestionDtoValidator : AbstractValidator<CreateQuestionDto>
{
    public CreateQuestionDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required");

        RuleFor(x => x.Type)
            .NotNull()
            .WithMessage("Type is required");

        RuleFor(x => x.Position)
            .NotEmpty()
            .WithMessage("Position is required")
            .GreaterThan(0)
            .WithMessage("Position must be greater than 0");

        RuleFor(x => x.ValueScore)
            .NotEmpty()
            .WithMessage("Value Score is required")
            .InclusiveBetween(1, 100)
            .WithMessage("Value Score must be between 1 and 100");
    }
}
