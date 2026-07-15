using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.Validators.Option;

public sealed class UpdateOptionDtoValidator : AbstractValidator<UpdateOptionDto>
{
    public UpdateOptionDtoValidator()
    {
        RuleFor(x => x.OptionId)
            .NotEmpty()
            .WithMessage("OptionId is required");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description cannot be empty")
            .When(x => x.Description is not null);

        RuleFor(x => x.Position)
            .GreaterThan(0)
            .WithMessage("Position must be greater than 0")
            .When(x => x.Position.HasValue);
    }
}
