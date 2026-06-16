using FluentValidation.TestHelper;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Validators;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Tests.Validators;

public class MatchQueryParametersValidatorTests
{
    private readonly MatchQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_ValidParameters_NoErrors()
    {
        var model = new MatchQueryParametersDto
        {
            Code = "ABC123",
            Mode = MatchMode.Single,
            Status = MatchStatus.Active,
            CourseId = Guid.NewGuid(),
            QuizId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllNull_NoErrors()
    {
        var model = new MatchQueryParametersDto();

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
