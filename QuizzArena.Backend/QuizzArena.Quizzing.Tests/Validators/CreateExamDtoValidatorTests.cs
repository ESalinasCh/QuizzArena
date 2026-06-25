using FluentValidation.TestHelper;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Validators.Quiz;
using QuizzArena.Quizzing.Domain.Enums;


namespace QuizzArena.Quizzing.Tests.Validators;

public class CreateExamDtoValidatorTests
{
    private readonly CreateExamDtoValidator _validator = new();

    private static CreateExamDto CreateValidDto() => new()
    {
        Title = "Docker Quiz",
        Description = "Questions about local infrastructure",
        Status = QuizStatus.draft,
        QuestionIds = [Guid.NewGuid(), Guid.NewGuid()]
    };

    // ── Title ───────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyTitle_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Title = string.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_TitleExceeds100Chars_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Title = new string('a', 101);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_TitleExactly100Chars_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Title = new string('a', 100);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    // ── Description ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyDescription_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Description = string.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionExceeds255Chars_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Description = new string('a', 256);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionExactly255Chars_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Description = new string('a', 255);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // ── Status ──────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_InvalidStatus_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Status = (QuizStatus)99;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData(QuizStatus.draft)]
    [InlineData(QuizStatus.published)]
    [InlineData(QuizStatus.archived)]
    public void Validate_ValidStatus_ShouldNotHaveValidationError(QuizStatus status)
    {
        var dto = CreateValidDto();
        dto.Status = status;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    // ── QuestionIds ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyQuestionIds_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.QuestionIds = [];

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.QuestionIds);
    }

    [Fact]
    public void Validate_EmptyQuestionIds_ShouldNotValidateEachElement()
    {
        // Cascade(Stop) on the collection means RuleForEach should not run
        var dto = CreateValidDto();
        dto.QuestionIds = [];

        var result = _validator.TestValidate(dto);

        // Only one error (the NotEmpty), not additional per-element errors
        Assert.Single(result.Errors.Where(e => e.PropertyName.StartsWith(nameof(dto.QuestionIds))));
    }

    [Fact]
    public void Validate_QuestionIdIsGuidEmpty_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.QuestionIds = [Guid.Empty];

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("QuestionIds[0]");
    }

    [Fact]
    public void Validate_MixedValidAndEmptyGuids_ShouldHaveValidationErrorOnlyForEmptyOnes()
    {
        var dto = CreateValidDto();
        dto.QuestionIds = [Guid.NewGuid(), Guid.Empty, Guid.NewGuid()];

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("QuestionIds[1]");
        result.ShouldNotHaveValidationErrorFor("QuestionIds[0]");
        result.ShouldNotHaveValidationErrorFor("QuestionIds[2]");
    }

    // ── Full valid DTO ───────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidDto_ShouldNotHaveAnyValidationErrors()
    {
        var dto = CreateValidDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
