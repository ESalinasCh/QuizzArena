using FluentValidation.TestHelper;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Validators.Match;

namespace QuizzArena.Quizzing.Tests.Validators;

public class CreateMatchDtoValidatorTests
{
    private readonly CreateMatchDtoValidator _validator = new();

    private static MatchCreateDto CreateValidDto() => new()
    {
        StartedAt = DateTimeOffset.UtcNow.AddHours(1),
        FinishedAt = DateTimeOffset.UtcNow.AddHours(2),
        TimeMinutes = 30,
        AttemptsAmount = 1,
        QuizId = Guid.NewGuid(),
        CourseId = Guid.NewGuid()
    };

    // ── Full valid DTO ───────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidDto_ShouldNotHaveAnyValidationErrors()
    {
        var dto = CreateValidDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── StartedAt ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_DefaultStartedAt_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.StartedAt = default;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartedAt);
    }

    // ── FinishedAt ───────────────────────────────────────────────────────────

    [Fact]
    public void Validate_DefaultFinishedAt_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.FinishedAt = default;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FinishedAt);
    }

    [Fact]
    public void Validate_FinishedAtEqualToStartedAt_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        var sameTime = DateTimeOffset.UtcNow.AddHours(1);
        dto.StartedAt = sameTime;
        dto.FinishedAt = sameTime;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FinishedAt);
    }

    [Fact]
    public void Validate_FinishedAtBeforeStartedAt_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.StartedAt = DateTimeOffset.UtcNow.AddHours(2);
        dto.FinishedAt = DateTimeOffset.UtcNow.AddHours(1);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FinishedAt);
    }

    [Fact]
    public void Validate_FinishedAtAfterStartedAt_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.FinishedAt);
    }

    // ── TimeMinutes ──────────────────────────────────────────────────────────

    [Fact]
    public void Validate_TimeMinutesZero_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.TimeMinutes = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.TimeMinutes);
    }

    [Fact]
    public void Validate_TimeMinutesNegative_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.TimeMinutes = -5;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.TimeMinutes);
    }

    [Fact]
    public void Validate_TimeMinutesPositive_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.TimeMinutes = 60;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.TimeMinutes);
    }

    // ── QuestionsAmount (optional) ───────────────────────────────────────────

    [Fact]
    public void Validate_QuestionsAmountNull_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.QuestionsAmount = null;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.QuestionsAmount);
    }

    [Fact]
    public void Validate_QuestionsAmountPositive_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.QuestionsAmount = 10;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.QuestionsAmount);
    }

    [Fact]
    public void Validate_QuestionsAmountZero_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.QuestionsAmount = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.QuestionsAmount);
    }

    [Fact]
    public void Validate_QuestionsAmountNegative_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.QuestionsAmount = -1;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.QuestionsAmount);
    }

    // ── AttemptsAmount ───────────────────────────────────────────────────────

    [Fact]
    public void Validate_AttemptsAmountZero_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.AttemptsAmount = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AttemptsAmount);
    }

    [Fact]
    public void Validate_AttemptsAmountNegative_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.AttemptsAmount = -1;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AttemptsAmount);
    }

    [Fact]
    public void Validate_AttemptsAmountPositive_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.AttemptsAmount = 3;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.AttemptsAmount);
    }

    // ── QuizId ───────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_QuizIdEmpty_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.QuizId = Guid.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.QuizId);
    }

    [Fact]
    public void Validate_QuizIdValid_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.QuizId = Guid.NewGuid();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.QuizId);
    }

    // ── CourseId ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_CourseIdEmpty_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.CourseId = Guid.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CourseId);
    }

    [Fact]
    public void Validate_CourseIdValid_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.CourseId = Guid.NewGuid();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.CourseId);
    }
}
