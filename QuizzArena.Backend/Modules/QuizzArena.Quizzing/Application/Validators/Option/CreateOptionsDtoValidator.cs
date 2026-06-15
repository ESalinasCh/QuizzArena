using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.Validators.Option;

public class CreateOptionsDtoValidator : AbstractValidator<IEnumerable<CreateOptionDto>>
{
    public CreateOptionsDtoValidator(CreateOptionDtoValidator validator)
    {
        RuleForEach(x => x)
            .SetValidator(validator);
    }
}
