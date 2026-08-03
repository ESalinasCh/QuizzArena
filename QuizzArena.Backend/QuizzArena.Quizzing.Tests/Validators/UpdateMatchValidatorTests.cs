using FluentValidation.TestHelper;
using QuizzArena.Quizzing.Application.Validators.Match;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Tests.Validators;

public class UpdateMatchValidatorTests
{
    private readonly UpdateMatchValidator _validator = new();

    private static Match CreateValidMatch() => new()
    {
        Id = Guid.NewGuid(),
        Title = "Docker Exam",
        StartedAt = DateTimeOffset.UtcNow,
        FinishedAt = DateTimeOffset.UtcNow.AddHours(1),
        TimeMinutes = 60,
        QuestionsAmount = 10,
        AttemptsAmount = 3,
        CourseId = Guid.NewGuid()
    };

    // ── Title ───────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_TitleExceeds200Chars_ShouldHaveValidationError()
    {
        var match = CreateValidMatch();
        match.Title = new string('a', 201);

        var result = _validator.TestValidate(match);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_TitleWith200Chars_ShouldNotHaveValidationError()
    {
        var match = CreateValidMatch();
        match.Title = new string('a', 200);

        var result = _validator.TestValidate(match);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    // ── FinishedAt ────────────────────────────────────────────────────────

    [Fact]
    public void Validate_FinishedAtBeforeStartedAt_ShouldHaveValidationError()
    {
        var match = CreateValidMatch();

        match.FinishedAt = match.StartedAt.AddMinutes(-10);

        var result = _validator.TestValidate(match);

        result.ShouldHaveValidationErrorFor(x => x.FinishedAt);
    }

    [Fact]
    public void Validate_FinishedAtAfterStartedAt_ShouldNotHaveValidationError()
    {
        var match = CreateValidMatch();

        var result = _validator.TestValidate(match);

        result.ShouldNotHaveValidationErrorFor(x => x.FinishedAt);
    }

    // ── TimeMinutes ────────────────────────────────────────────────────────

    [Fact]
    public void Validate_TimeMinutesLessThanOne_ShouldHaveValidationError()
    {
        var match = CreateValidMatch();
        match.TimeMinutes = 0;

        var result = _validator.TestValidate(match);

        result.ShouldHaveValidationErrorFor(x => x.TimeMinutes);
    }

    // ── QuestionsAmount ───────────────────────────────────────────────────

    [Fact]
    public void Validate_QuestionsAmountLessThanOne_ShouldHaveValidationError()
    {
        var match = CreateValidMatch();
        match.QuestionsAmount = 0;

        var result = _validator.TestValidate(match);

        result.ShouldHaveValidationErrorFor(x => x.QuestionsAmount);
    }

    // ── AttemptsAmount ────────────────────────────────────────────────────

    [Fact]
    public void Validate_AttemptsAmountLessThanOne_ShouldHaveValidationError()
    {
        var match = CreateValidMatch();
        match.AttemptsAmount = 0;

        var result = _validator.TestValidate(match);

        result.ShouldHaveValidationErrorFor(x => x.AttemptsAmount);
    }

    // ── CourseId ──────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyCourseId_ShouldHaveValidationError()
    {
        var match = CreateValidMatch();
        match.CourseId = Guid.Empty;

        var result = _validator.TestValidate(match);

        result.ShouldHaveValidationErrorFor(x => x.CourseId);
    }
}
