using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.Validators.Question;

internal class CreateQuestionsDtoValidator : AbstractValidator<IEnumerable<CreateQuestionDto>>
{
    public CreateQuestionsDtoValidator(CreateQuestionDtoValidator validator)
    {
        RuleForEach(x => x)
            .SetValidator(validator);
    }
}
