using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Match;

namespace QuizzArena.Quizzing.Application.Validators.Match;

public class CreateMatchDtoValidator : AbstractValidator<MatchCreateDto>
{
    public CreateMatchDtoValidator()
    {
        RuleFor(x => x.StartedAt)
            .NotEmpty()
            .WithMessage("Start date is required")
            .GreaterThan(DateTimeOffset.Now);

        RuleFor(x => x.FinishedAt)
            .NotEmpty()
            .WithMessage("Finish date is required")
            .GreaterThan(x => x.StartedAt)
            .WithMessage("Finish date must be after the start date");

        RuleFor(x => x.TimeMinutes)
            .GreaterThan(0)
            .WithMessage("Time minutes must be greater than 0");

        RuleFor(x => x.QuestionsAmount)
            .GreaterThan(0)
            .When(x => x.QuestionsAmount.HasValue)
            .WithMessage("Questions amount must be greater than 0");

        RuleFor(x => x.AttemptsAmount)
            .GreaterThan(0)
            .WithMessage("Attempts amount must be greater than 0");

        RuleFor(x => x.QuizId)
            .NotEqual(Guid.Empty)
            .WithMessage("Quiz ID must be a valid GUID");

        RuleFor(x => x.CourseId)
            .NotEqual(Guid.Empty)
            .WithMessage("Course ID must be a valid GUID");

    }
}
