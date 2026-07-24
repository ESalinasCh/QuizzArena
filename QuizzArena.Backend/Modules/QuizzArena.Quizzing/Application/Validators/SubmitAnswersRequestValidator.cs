using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;

namespace QuizzArena.Quizzing.Application.Validators;

public class SubmitAnswersRequestValidator : AbstractValidator<SubmitAnswersRequestDto>
{
    public SubmitAnswersRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("At least one answer is required.")
            // One entry per question: a repeated question means the client sent
            // the same answer twice, not a multi-option pick.
            .Must(answers => answers.Select(answer => answer.QuestionId).Distinct().Count() == answers.Count)
            .WithMessage("Each question can only be answered once.");

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

        RuleFor(x => x.SelectedOptionIds)
            .NotEmpty().WithMessage("At least one selected option is required.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("SelectedOptionIds cannot contain an empty id.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("SelectedOptionIds cannot contain duplicates.");

        RuleFor(x => x.AnsweredAt)
            .NotEmpty().WithMessage("AnsweredAt is required.")
            .Must(at => at <= DateTimeOffset.UtcNow)
            .WithMessage("AnsweredAt cannot be in the future.");
    }
}
