using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Quiz;

namespace QuizzArena.Quizzing.Application.Validators.Quiz;

public class CreateExamDtoValidator : AbstractValidator<CreateExamDto>
{
    public CreateExamDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(100)
            .WithMessage("Title must not exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(255)
            .WithMessage("Description must not exceed 255 characters");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status has an invalid value");

        RuleFor(x => x.QuestionIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("At least one question is required");

        RuleForEach(x => x.QuestionIds)
            .NotEqual(Guid.Empty)
            .WithMessage("Question IDs must be valid GUIDs");
    }
}
