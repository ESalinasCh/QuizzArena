using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Quiz;

namespace QuizzArena.Quizzing.Application.Validators.Quiz;

public class CreateQuizDtoValidator : AbstractValidator<CreateQuizDto>
{
    public CreateQuizDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required");
    }
}
