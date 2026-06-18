
using FluentValidation;
using QuizzArena.Quizzing.Application.Filters;

namespace QuizzArena.Quizzing.Application.Validators.FiltersValidators;

public class MatchAttemptFiltersValidator : AbstractValidator<MatchAttemptFilters>
{
    public MatchAttemptFiltersValidator()
    {
        RuleFor(x => x.Page)
             .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.MinScore)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinScore.HasValue);

        RuleFor(x => x.MaxScore)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxScore.HasValue);

        RuleFor(x => x.MaxScore)
            .GreaterThanOrEqualTo(x => x.MinScore!.Value)
            .When(x => x.MinScore.HasValue && x.MaxScore.HasValue)
            .WithMessage("MaxScore must be greater than or equal to MinScore.");

        RuleFor(x => x.StartedTo)
            .GreaterThanOrEqualTo(x => x.StartedFrom!.Value)
            .When(x => x.StartedFrom.HasValue && x.StartedTo.HasValue)
            .WithMessage("StartedTo must be greater than or equal to StartedFrom.");

    }
}
