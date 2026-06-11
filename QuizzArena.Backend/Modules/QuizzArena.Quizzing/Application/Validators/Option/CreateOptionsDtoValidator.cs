using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.Validators.Option;

internal class CreateOptionsDtoValidator : AbstractValidator<IEnumerable<CreateOptionDto>>
{
    public CreateOptionsDtoValidator(CreateOptionDtoValidator validator)
    {
        RuleForEach(x => x)
            .SetValidator(validator);
    }
}
