
using FluentValidation.TestHelper;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Validators.Match;

namespace QuizzArena.Quizzing.Tests.Validators;

public class UpdateMatchDtoValidatorTests
{
    private readonly UpdateMatchDtoValidator _validator = new();

    private static MatchUpdateDto CreateValidDto() => new()
    {
        Title = "Updated Match",
        StartedAt = DateTimeOffset.UtcNow.AddHours(1),
        FinishedAt = DateTimeOffset.UtcNow.AddHours(2),
        TimeMinutes = 60,
        QuestionsAmount = 5,
        AttemptsAmount = 3,
        ShuffleQuestion = true,
        ShuffleOptions = true
    };

    // ── Empty Update ───────────────────────────────────────────────────────

    [Fact]
    public void Validate_NoFieldsProvided_ShouldHaveValidationError()
    {
        var dto = new MatchUpdateDto();

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    // ── Title ───────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_TitleExceeds200Chars_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Title = new string('a', 201);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_TitleWith200Chars_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Title = new string('a', 200);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    // ── TimeMinutes ────────────────────────────────────────────────────────

    [Fact]
    public void Validate_TimeMinutesLessThanOne_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.TimeMinutes = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.TimeMinutes);
    }

    [Fact]
    public void Validate_TimeMinutesPositive_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.TimeMinutes = 1;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.TimeMinutes);
    }

    // ── QuestionsAmount ───────────────────────────────────────────────────

    [Fact]
    public void Validate_QuestionsAmountLessThanOne_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.QuestionsAmount = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.QuestionsAmount);
    }

    // ── AttemptsAmount ────────────────────────────────────────────────────

    [Fact]
    public void Validate_AttemptsAmountLessThanOne_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.AttemptsAmount = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AttemptsAmount);
    }

    // ── StartedAt ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_StartedAtInThePast_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.StartedAt = DateTimeOffset.UtcNow.AddMinutes(-5);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartedAt);
    }

    [Fact]
    public void Validate_StartedAtBeforeFinishedAt_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();

        dto.StartedAt = DateTimeOffset.UtcNow.AddHours(1);
        dto.FinishedAt = DateTimeOffset.UtcNow.AddHours(2);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    // ── FinishedAt ────────────────────────────────────────────────────────

    [Fact]
    public void Validate_FinishedAtLessThanOneMinuteInFuture_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.FinishedAt = DateTimeOffset.UtcNow.AddSeconds(30);

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

        result.ShouldHaveValidationErrorFor(x => x);
    }
}
