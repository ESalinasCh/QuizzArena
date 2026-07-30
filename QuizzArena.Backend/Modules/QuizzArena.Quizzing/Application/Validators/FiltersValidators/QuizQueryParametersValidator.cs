using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Quiz;

namespace QuizzArena.Quizzing.Application.Validators.FiltersValidators;

public class QuizQueryParametersValidator : AbstractValidator<QuizQueryParametersDto>
{
    public QuizQueryParametersValidator()
    {
        RuleFor(x => x.Origin)
            .IsInEnum()
            .When(x => x.Origin.HasValue)
            .WithMessage("Invalid quiz origin.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid quiz status.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize must be greater than or equal to 1.");
    }
}
