using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Match;

namespace QuizzArena.Quizzing.Application.Validators.Match;

public class UpdateMatchDtoValidator : AbstractValidator<MatchUpdateDto>
{
    public UpdateMatchDtoValidator()
    {
        RuleFor(x => x)
        .Must(x =>
            x.Title != null ||
            x.StartedAt.HasValue ||
            x.FinishedAt.HasValue ||
            x.TimeMinutes.HasValue ||
            x.QuestionsAmount.HasValue ||
            x.AttemptsAmount.HasValue ||
            x.ShuffleQuestion.HasValue ||
            x.ShuffleOptions.HasValue
            )
        .WithMessage("At least one field must be modified.");


        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.TimeMinutes)
            .GreaterThan(0)
            .When(x => x.TimeMinutes.HasValue);

        RuleFor(x => x.QuestionsAmount)
            .GreaterThan(0)
            .When(x => x.QuestionsAmount.HasValue);

        RuleFor(x => x.AttemptsAmount)
            .GreaterThan(0)
            .When(x => x.AttemptsAmount.HasValue);

        RuleFor(x => x.StartedAt)
            .Must(startedAt => startedAt >= DateTimeOffset.UtcNow)
            .When(x => x.StartedAt is not null)
            .WithMessage("StartedAt must not be in the past.");

        RuleFor(x => x)
            .Must(dto => dto.StartedAt! < dto.FinishedAt!)
            .When(dto => dto.StartedAt is not null && dto.FinishedAt is not null)
            .WithMessage("StartedAt must be earlier than FinishedAt.");

        RuleFor(x => x.FinishedAt)
            .Must(finishedAt => finishedAt >= DateTimeOffset.UtcNow.AddMinutes(1))
            .When(x => x.FinishedAt.HasValue)
            .WithMessage("FinishedAt must be at least one minute in the future.");
    }
}
