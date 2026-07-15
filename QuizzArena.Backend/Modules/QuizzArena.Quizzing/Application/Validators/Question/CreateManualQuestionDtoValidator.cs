using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Validators.Option;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.Validators.Question;

public class CreateManualQuestionDtoValidator : AbstractValidator<CreateManualQuestionDto>
{
    private const int MaxOptions = 4;

    public CreateManualQuestionDtoValidator(CreateOptionDtoValidator optionValidator)
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required");

        RuleFor(x => x.Type)
            .NotNull()
            .WithMessage("Type is required");

        RuleFor(x => x.ProcessingJobId)
            .NotEmpty()
            .WithMessage("ProcessingJobId is required");

        RuleFor(x => x.Options)
            .NotEmpty()
            .WithMessage("At least one option is required")
            .Must(options => options.Count <= MaxOptions)
            .WithMessage($"A question can have at most {MaxOptions} options");

        RuleForEach(x => x.Options)
            .SetValidator(optionValidator);

        RuleFor(x => x.Options)
            .Must(options => options.Any(o => o.IsCorrect))
            .WithMessage("At least one option must be correct");

        RuleFor(x => x.Options)
            .Must((dto, options) => options.Count(o => o.IsCorrect) == 1)
            .When(x => x.Type is QuestionType.SingleChoice or QuestionType.TrueFalse)
            .WithMessage("A single-answer question must have exactly one correct option");

        RuleFor(x => x.Options)
            .Must(HasConsecutivePositions)
            .WithMessage("Option positions must be consecutive starting from 1 (e.g. 1, 2, 3)");
    }

    private static bool HasConsecutivePositions(List<CreateOptionDto> options)
    {
        int[] positions = options.Select(o => o.Position).OrderBy(p => p).ToArray();
        for (int i = 0; i < positions.Length; i++)
        {
            if (positions[i] != i + 1)
            {
                return false;
            }
        }

        return true;
    }
}
