using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;

namespace QuizzArena.Quizzing.Application.Validators;

public class SubmitAnswersRequestValidator : AbstractValidator<SubmitAnswersRequestDto>
{
    public SubmitAnswersRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("At least one answer is required.");

        RuleForEach(x => x.Answers)
            .SetValidator(new SubmitAnswerBodyValidator());
    }
}

public class SubmitAnswerBodyValidator : AbstractValidator<SubmitAnswerBody>
{
    public SubmitAnswerBodyValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("QuestionId is required.");

        RuleFor(x => x.SelectedOptionId)
            .NotEmpty().WithMessage("SelectedOptionId is required.");

        RuleFor(x => x.AnsweredAt)
            .NotEmpty().WithMessage("AnsweredAt is required.")
            .Must(at => at <= DateTimeOffset.UtcNow)
            .WithMessage("AnsweredAt cannot be in the future.");
    }
}
