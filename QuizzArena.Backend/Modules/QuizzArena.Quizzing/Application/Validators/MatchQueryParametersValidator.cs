using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Match;

namespace QuizzArena.Quizzing.Application.Validators;

public class MatchQueryParametersValidator : AbstractValidator<MatchQueryParametersDto>
{
    public MatchQueryParametersValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid match status.");

        RuleFor(x => x.Mode)
            .IsInEnum()
            .When(x => x.Mode.HasValue)
            .WithMessage("Invalid match mode.");

        RuleFor(x => x.CourseId)
            .NotEqual(Guid.Empty)
            .When(x => x.CourseId.HasValue)
            .WithMessage("Invalid course id.");

        RuleFor(x => x.QuizId)
            .NotEqual(Guid.Empty)
            .When(x => x.QuizId.HasValue)
            .WithMessage("Invalid quiz id.");
    }

}
