using FluentValidation;
using MatchEntity = QuizzArena.Quizzing.Domain.Entities.Match;

namespace QuizzArena.Quizzing.Application.Validators.Match;

public class UpdateMatchValidator : AbstractValidator<MatchEntity>
{
    public UpdateMatchValidator()
    {
        RuleFor(x => x.Title)
           .MaximumLength(200)
           .When(x => x.Title is not null);

        RuleFor(x => x.FinishedAt)
            .GreaterThan(x => x.StartedAt)
            .When(x => x.FinishedAt.HasValue)
            .WithMessage("FinishedAt must be later than StartedAt.");

        RuleFor(x => x.TimeMinutes)
            .GreaterThan(0);

        RuleFor(x => x.QuestionsAmount)
            .GreaterThan(0)
            .When(x => x.QuestionsAmount.HasValue);

        RuleFor(x => x.AttemptsAmount)
            .GreaterThan(0);

        RuleFor(x => x.CourseId)
            .NotEmpty();
    }
}
