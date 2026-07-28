using FluentValidation.TestHelper;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Validators;

namespace QuizzArena.Quizzing.Tests.Validators;

public class SubmitAnswersRequestValidatorTests
{
    private readonly SubmitAnswersRequestValidator _validator = new();

    private static SubmitAnswersRequestDto CreateValidRequest() => new()
    {
        Answers = [new SubmitAnswerBody(Guid.NewGuid(), [Guid.NewGuid()], DateTimeOffset.UtcNow.AddMinutes(-1))]
    };

    [Fact]
    public void Validate_EmptyAnswers_ShouldHaveValidationError()
    {
        SubmitAnswersRequestDto request = CreateValidRequest();
        request.Answers = [];

        TestValidationResult<SubmitAnswersRequestDto> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Answers);
    }

    [Fact]
    public void Validate_EmptyQuestionId_ShouldHaveValidationError()
    {
        SubmitAnswersRequestDto request = new()
        {
            Answers = [new SubmitAnswerBody(Guid.Empty, [Guid.NewGuid()], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        TestValidationResult<SubmitAnswersRequestDto> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Answers[0].QuestionId");
    }

    [Fact]
    public void Validate_SameQuestionAnsweredTwice_ShouldHaveValidationError()
    {
        Guid questionId = Guid.NewGuid();

        // A multi-option answer is one entry with several ids, never two entries.
        SubmitAnswersRequestDto request = new()
        {
            Answers =
            [
                new SubmitAnswerBody(questionId, [Guid.NewGuid()], DateTimeOffset.UtcNow.AddMinutes(-1)),
                new SubmitAnswerBody(questionId, [Guid.NewGuid()], DateTimeOffset.UtcNow.AddMinutes(-1))
            ]
        };

        TestValidationResult<SubmitAnswersRequestDto> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Answers);
    }

    [Fact]
    public void Validate_NoSelectedOptionIds_ShouldHaveValidationError()
    {
        SubmitAnswersRequestDto request = new()
        {
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), [], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        TestValidationResult<SubmitAnswersRequestDto> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Answers[0].SelectedOptionIds");
    }

    [Fact]
    public void Validate_EmptySelectedOptionId_ShouldHaveValidationError()
    {
        SubmitAnswersRequestDto request = new()
        {
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), [Guid.Empty], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        TestValidationResult<SubmitAnswersRequestDto> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Answers[0].SelectedOptionIds");
    }

    [Fact]
    public void Validate_DuplicatedSelectedOptionIds_ShouldHaveValidationError()
    {
        Guid optionId = Guid.NewGuid();

        SubmitAnswersRequestDto request = new()
        {
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), [optionId, optionId], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        TestValidationResult<SubmitAnswersRequestDto> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Answers[0].SelectedOptionIds");
    }

    [Fact]
    public void Validate_AnsweredAtInFuture_ShouldHaveValidationError()
    {
        SubmitAnswersRequestDto request = new()
        {
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), [Guid.NewGuid()], DateTimeOffset.UtcNow.AddMinutes(5))]
        };

        TestValidationResult<SubmitAnswersRequestDto> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Answers[0].AnsweredAt");
    }

    [Fact]
    public void Validate_MultipleSelectedOptionIds_ShouldNotHaveValidationErrors()
    {
        SubmitAnswersRequestDto request = new()
        {
            Answers = [new SubmitAnswerBody(
                Guid.NewGuid(),
                [Guid.NewGuid(), Guid.NewGuid()],
                DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        TestValidationResult<SubmitAnswersRequestDto> result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        TestValidationResult<SubmitAnswersRequestDto> result = _validator.TestValidate(CreateValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
