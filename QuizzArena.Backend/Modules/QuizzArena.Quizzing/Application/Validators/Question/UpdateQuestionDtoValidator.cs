using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Validators.Option;

namespace QuizzArena.Quizzing.Application.Validators.Question;

public class UpdateQuestionDtoValidator : AbstractValidator<UpdateQuestionDto>
{
    public UpdateQuestionDtoValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage("QuestionId is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content cannot be empty")
            .When(x => x.Content is not null);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status is invalid")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type is invalid")
            .When(x => x.Type.HasValue);

        RuleForEach(x => x.Options)
            .SetValidator(new UpdateOptionDtoValidator());
    }
}
