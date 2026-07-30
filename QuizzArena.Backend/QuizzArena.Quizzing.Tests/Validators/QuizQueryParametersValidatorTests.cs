using FluentValidation.TestHelper;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Validators.FiltersValidators;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Tests.Validators;

public class QuizQueryParametersValidatorTests
{
    private readonly QuizQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_ValidParameters_NoErrors()
    {
        var model = new QuizQueryParametersDto
        {
            Origin = QuizOrigin.ManuallyCreated,
            Status = QuizStatus.published,
            Page = 2,
            PageSize = 10,
            Search = "math"
        };

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllFiltersNull_NoErrors()
    {
        var model = new QuizQueryParametersDto();

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(QuizStatus.draft)]
    [InlineData(QuizStatus.published)]
    [InlineData(QuizStatus.archived)]
    public void Validate_EachKnownStatus_NoErrors(QuizStatus status)
    {
        var model = new QuizQueryParametersDto { Status = status };

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_StatusNotInEnum_HasError()
    {
        var model = new QuizQueryParametersDto { Status = (QuizStatus)99 };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Invalid quiz status.");
    }

    [Fact]
    public void Validate_OriginNotInEnum_HasError()
    {
        var model = new QuizQueryParametersDto { Origin = (QuizOrigin)99 };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Origin)
            .WithErrorMessage("Invalid quiz origin.");
    }
}
