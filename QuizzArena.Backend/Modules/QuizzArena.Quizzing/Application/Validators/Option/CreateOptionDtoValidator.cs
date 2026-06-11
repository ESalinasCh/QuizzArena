using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.Validators.Option;

internal class CreateOptionDtoValidator : AbstractValidator<CreateOptionDto>
{
    public CreateOptionDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");
    }
}
