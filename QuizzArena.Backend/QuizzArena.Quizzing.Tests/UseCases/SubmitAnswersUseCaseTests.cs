
using AutoMapper;
using FluentValidation;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;
using QuizzArena.Quizzing.Application.Validators;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Domain.Exceptions;
using Shared.Contracts;
using DomainMatch = QuizzArena.Quizzing.Domain.Entities.Match;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class SubmitAnswersUseCaseTests
{
    // Mocks
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IMatchAttemptRepository> _mockMatchAttemptRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<ICurrentUser> _mockCurrentUser;

    // Real
    private readonly SubmitAnswersRequestValidator _submitAnswersValidator;

    // Target unit test
    private readonly SubmitAnswersUseCase _submitAnswersUseCase;

    public SubmitAnswersUseCaseTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockMatchAttemptRepository = new Mock<IMatchAttemptRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _submitAnswersValidator = new SubmitAnswersRequestValidator();

        _submitAnswersUseCase = new SubmitAnswersUseCase(
            _submitAnswersValidator,
            _mockMatchRepository.Object,
            _mockMatchAttemptRepository.Object,
            _mockMapper.Object,
            _mockQuestionRepository.Object,
            _mockCurrentUser.Object
        );
    }

    [Fact]
    public async Task SubmitAnswer_ValidRequest_ReturnsResult()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var matchAttemptId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();

        _mockCurrentUser.Setup(user => user.UserId).Returns(userId);

        var matchAttempt = new MatchAttempt { Id = matchAttemptId, MatchId = matchId, UserId = Guid.Parse(userId) };
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetByIdAsync(matchAttemptId))
            .ReturnsAsync(matchAttempt);
        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new DomainMatch { Id = matchId, AttemptsAmount = 1 });
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetMatchAttemptCountByMatchIdAndUserIdAsync(matchId, Guid.Parse(userId)))
            .ReturnsAsync(1);

        var dto = new SubmitAnswersRequestDto
        {
            Answers = [new SubmitAnswerBody(questionId, [optionId], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        var answers = new List<Answer>
        {
            new() { QuestionId = questionId, SelectedOptions = [new SelectedOption { OptionId = optionId }] }
        };
        _mockMapper.Setup(m => m.Map<List<Answer>>(dto.Answers)).Returns(answers);

        var option = new Option { Id = optionId, IsCorrect = true };

        _mockMatchAttemptRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync(matchAttempt);

        var question = new Question { Id = questionId, Content = "Sample question", Type = QuestionType.SingleChoice, Options = [option] };
        _mockQuestionRepository
            .Setup(repo => repo.GetByIdsWithOptionsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([question]);

        // Act
        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(matchAttemptId, result.AttemptId);
        Assert.Equal(100, result.ScorePercentage);
        Assert.Equal(1, result.CorrectCount);
        Assert.Equal(0, result.IncorrectCount);
        Assert.Equal(1, result.TotalQuestions);
        Assert.Single(result.Questions);
        Assert.True(result.Questions[0].IsCorrect);
    }

    /// <summary>
    /// Wires up the happy-path repositories and returns the attempt id for a
    /// single question of <paramref name="type"/> whose options are <paramref name="options"/>.
    /// The attempt is handed back so tests can inspect what was persisted.
    /// </summary>
    private (Guid MatchAttemptId, SubmitAnswersRequestDto Dto, MatchAttempt MatchAttempt) ArrangeSingleQuestion(
        QuestionType type,
        List<Option> options,
        List<Guid> selectedOptionIds)
    {
        string userId = Guid.NewGuid().ToString();
        var matchAttemptId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        _mockCurrentUser.Setup(user => user.UserId).Returns(userId);

        var matchAttempt = new MatchAttempt { Id = matchAttemptId, MatchId = matchId, UserId = Guid.Parse(userId) };
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetByIdAsync(matchAttemptId))
            .ReturnsAsync(matchAttempt);
        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new DomainMatch { Id = matchId, AttemptsAmount = 1 });
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetMatchAttemptCountByMatchIdAndUserIdAsync(matchId, Guid.Parse(userId)))
            .ReturnsAsync(1);
        _mockMatchAttemptRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync(matchAttempt);

        // One request entry per question, carrying every option the student picked.
        var dto = new SubmitAnswersRequestDto
        {
            Answers = [new SubmitAnswerBody(questionId, selectedOptionIds, DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        // Stands in for what SubmitAnswersMappingProfile produces (the mapper is mocked).
        List<Answer> answers =
        [
            new Answer
            {
                QuestionId = questionId,
                SelectedOptions = [.. selectedOptionIds.Select(id => new SelectedOption { OptionId = id })]
            }
        ];
        _mockMapper.Setup(m => m.Map<List<Answer>>(dto.Answers)).Returns(answers);

        var question = new Question { Id = questionId, Content = "Sample question", Type = type, Options = options };
        _mockQuestionRepository
            .Setup(repo => repo.GetByIdsWithOptionsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([question]);

        return (matchAttemptId, dto, matchAttempt);
    }

    [Fact]
    public async Task SubmitAnswer_MultipleChoiceAllCorrectOptionsSelected_ScoresCorrect()
    {
        Option a = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option b = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option c = new() { Id = Guid.NewGuid(), IsCorrect = false };

        (Guid matchAttemptId, SubmitAnswersRequestDto dto, _) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, [a, b, c], [a.Id, b.Id]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(100, result.ScorePercentage);
        Assert.Equal(1, result.CorrectCount);
        Assert.Equal(1, result.TotalQuestions);
        Assert.Single(result.Questions);
        Assert.True(result.Questions[0].IsCorrect);
        Assert.True(result.Questions[0].CorrectOptionIds.ToHashSet().SetEquals([a.Id, b.Id]));
        Assert.True(result.Questions[0].SelectedOptionIds.ToHashSet().SetEquals([a.Id, b.Id]));
    }

    [Fact]
    public async Task SubmitAnswer_MultipleChoicePartialSelection_ScoresIncorrect()
    {
        Option a = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option b = new() { Id = Guid.NewGuid(), IsCorrect = true };

        // Only one of the two correct options — all-or-nothing means no points.
        (Guid matchAttemptId, SubmitAnswersRequestDto dto, _) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, [a, b], [a.Id]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(0, result.ScorePercentage);
        Assert.Equal(1, result.IncorrectCount);
        Assert.Equal(1, result.TotalQuestions);
        Assert.False(result.Questions[0].IsCorrect);
    }

    [Fact]
    public async Task SubmitAnswer_MultipleChoiceOverSelection_ScoresIncorrect()
    {
        Option a = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option b = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option c = new() { Id = Guid.NewGuid(), IsCorrect = false };

        // Both correct options plus a wrong one — selecting everything must not pass.
        (Guid matchAttemptId, SubmitAnswersRequestDto dto, _) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, [a, b, c], [a.Id, b.Id, c.Id]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(0, result.ScorePercentage);
        Assert.False(result.Questions[0].IsCorrect);
    }

    /// <summary>
    /// Builds <paramref name="correctCount"/> correct options followed by
    /// <paramref name="wrongCount"/> wrong ones, so a test can describe a question shape
    /// like "3 correct, 1 wrong" without hand-writing every option.
    /// </summary>
    private static List<Option> BuildOptions(int correctCount, int wrongCount) =>
    [
        .. Enumerable.Range(0, correctCount).Select(_ => new Option { Id = Guid.NewGuid(), IsCorrect = true }),
        .. Enumerable.Range(0, wrongCount).Select(_ => new Option { Id = Guid.NewGuid(), IsCorrect = false })
    ];

    // The number of correct options per question is not fixed: a multiple-choice
    // question may have 2 of 4 correct, 3 of 4, or every option correct.
    [Theory]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    [InlineData(4, 0)]
    public async Task SubmitAnswer_MultipleChoiceEveryCorrectOptionSelected_ScoresCorrect(int correctCount, int wrongCount)
    {
        List<Option> options = BuildOptions(correctCount, wrongCount);
        List<Guid> everyCorrectId = [.. options.Where(option => option.IsCorrect).Select(option => option.Id)];

        (Guid matchAttemptId, SubmitAnswersRequestDto dto, MatchAttempt matchAttempt) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, options, everyCorrectId);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(100, result.ScorePercentage);
        Assert.Equal(1, result.CorrectCount);
        Assert.True(result.Questions[0].IsCorrect);
        Assert.Equal(correctCount, result.Questions[0].CorrectOptionIds.Count);
        Assert.All(Assert.Single(matchAttempt.Answers).SelectedOptions, selected => Assert.True(selected.IsCorrect));
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    [InlineData(4, 0)]
    public async Task SubmitAnswer_MultipleChoiceOneCorrectOptionMissing_ScoresIncorrect(int correctCount, int wrongCount)
    {
        List<Option> options = BuildOptions(correctCount, wrongCount);
        List<Guid> allCorrectButOne = [.. options.Where(option => option.IsCorrect).Select(option => option.Id).Skip(1)];

        (Guid matchAttemptId, SubmitAnswersRequestDto dto, _) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, options, allCorrectButOne);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(0, result.ScorePercentage);
        Assert.Equal(1, result.IncorrectCount);
        Assert.False(result.Questions[0].IsCorrect);
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    public async Task SubmitAnswer_MultipleChoiceEveryCorrectOptionPlusAWrongOne_ScoresIncorrect(int correctCount, int wrongCount)
    {
        List<Option> options = BuildOptions(correctCount, wrongCount);
        List<Guid> everyCorrectPlusAWrongOne =
        [
            .. options.Where(option => option.IsCorrect).Select(option => option.Id),
            options.First(option => !option.IsCorrect).Id
        ];

        (Guid matchAttemptId, SubmitAnswersRequestDto dto, MatchAttempt matchAttempt) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, options, everyCorrectPlusAWrongOne);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(0, result.ScorePercentage);
        Assert.False(result.Questions[0].IsCorrect);

        // The question is wrong as a whole, but the individual picks keep their own verdict.
        Answer answer = Assert.Single(matchAttempt.Answers);
        Assert.Equal(correctCount, answer.SelectedOptions.Count(selected => selected.IsCorrect));
        Assert.Equal(1, answer.SelectedOptions.Count(selected => !selected.IsCorrect));
    }

    [Fact]
    public async Task SubmitAnswer_SingleChoiceCorrectOptionSelected_ScoresCorrect()
    {
        List<Option> options = BuildOptions(correctCount: 1, wrongCount: 3);
        Guid correctOptionId = options.Single(option => option.IsCorrect).Id;

        (Guid matchAttemptId, SubmitAnswersRequestDto dto, _) = ArrangeSingleQuestion(
            QuestionType.SingleChoice, options, [correctOptionId]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(100, result.ScorePercentage);
        Assert.Equal(1, result.CorrectCount);
        Assert.True(result.Questions[0].IsCorrect);
        Assert.Equal(correctOptionId, Assert.Single(result.Questions[0].SelectedOptionIds));
    }

    [Fact]
    public async Task SubmitAnswer_SingleChoiceWrongOptionSelected_ScoresIncorrect()
    {
        List<Option> options = BuildOptions(correctCount: 1, wrongCount: 3);
        Guid wrongOptionId = options.First(option => !option.IsCorrect).Id;

        (Guid matchAttemptId, SubmitAnswersRequestDto dto, MatchAttempt matchAttempt) = ArrangeSingleQuestion(
            QuestionType.SingleChoice, options, [wrongOptionId]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(0, result.ScorePercentage);
        Assert.Equal(1, result.IncorrectCount);
        Assert.False(result.Questions[0].IsCorrect);
        Assert.False(Assert.Single(Assert.Single(matchAttempt.Answers).SelectedOptions).IsCorrect);
    }

    [Fact]
    public async Task SubmitAnswer_SingleChoiceMultipleOptionsSelected_ScoresIncorrect()
    {
        Option correct = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option wrong = new() { Id = Guid.NewGuid(), IsCorrect = false };

        // Hedging by picking both must not count as correct on a single-choice question.
        (Guid matchAttemptId, SubmitAnswersRequestDto dto, _) = ArrangeSingleQuestion(
            QuestionType.SingleChoice, [correct, wrong], [correct.Id, wrong.Id]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(0, result.ScorePercentage);
        Assert.False(result.Questions[0].IsCorrect);
    }

    [Fact]
    public async Task SubmitAnswer_TrueFalseCorrectOptionSelected_ScoresCorrect()
    {
        Option trueOption = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option falseOption = new() { Id = Guid.NewGuid(), IsCorrect = false };

        (Guid matchAttemptId, SubmitAnswersRequestDto dto, _) = ArrangeSingleQuestion(
            QuestionType.TrueFalse, [trueOption, falseOption], [trueOption.Id]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(100, result.ScorePercentage);
        Assert.True(result.Questions[0].IsCorrect);
    }

    [Fact]
    public async Task SubmitAnswer_MultiOptionAnswer_CountsAsOneQuestion()
    {
        Option a = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option b = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option c = new() { Id = Guid.NewGuid(), IsCorrect = true };

        // Three selected options, one question: the score denominator must be 1, not 3.
        (Guid matchAttemptId, SubmitAnswersRequestDto dto, MatchAttempt matchAttempt) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, [a, b, c], [a.Id, b.Id, c.Id]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(1, result.TotalQuestions);
        Assert.Equal(1, result.CorrectCount);
        Assert.Equal(0, result.IncorrectCount);
        Assert.Equal(100, result.ScorePercentage);
        Assert.Single(result.Questions);

        // One Answer row holding the three picks, not three Answer rows.
        Answer answer = Assert.Single(matchAttempt.Answers);
        Assert.Equal(3, answer.SelectedOptions.Count);
    }

    [Fact]
    public async Task SubmitAnswer_MixedSelection_FlagsEachSelectedOption()
    {
        Option correct = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option alsoCorrect = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option wrong = new() { Id = Guid.NewGuid(), IsCorrect = false };

        (Guid matchAttemptId, SubmitAnswersRequestDto dto, MatchAttempt matchAttempt) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, [correct, alsoCorrect, wrong], [correct.Id, wrong.Id]);

        await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Answer answer = Assert.Single(matchAttempt.Answers);
        Assert.True(answer.SelectedOptions.Single(selected => selected.OptionId == correct.Id).IsCorrect);
        Assert.False(answer.SelectedOptions.Single(selected => selected.OptionId == wrong.Id).IsCorrect);
    }

    [Fact]
    public async Task SubmitAnswer_OptionFromAnotherQuestion_ThrowsInvalidSelectedOptionException()
    {
        Option correct = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Guid foreignOptionId = Guid.NewGuid();

        // The option id is well-formed but belongs to no option of this question.
        (Guid matchAttemptId, SubmitAnswersRequestDto dto, _) = ArrangeSingleQuestion(
            QuestionType.SingleChoice, [correct], [foreignOptionId]);

        InvalidSelectedOptionException exception = await Assert.ThrowsAsync<InvalidSelectedOptionException>(
            () => _submitAnswersUseCase.Execute(matchAttemptId, dto)
        );

        Assert.Contains(foreignOptionId.ToString(), exception.Message);
    }

    [Fact]
    public async Task SubmitAnswer_InvalidMatchAttemptIdUserNotRegistered_ThrowsError()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        Guid anotherUserId = Guid.NewGuid();
        Guid matchAttemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId = Guid.NewGuid();
        MatchAttempt matchAttempt = new MatchAttempt
        {
            Id = matchAttemptId,
            UserId = anotherUserId
        };

        _mockCurrentUser.Setup(user => user.UserId).Returns(userId);
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetByIdAsync(matchAttemptId))
            .ReturnsAsync(matchAttempt);

        var dto = new SubmitAnswersRequestDto
        {
            Answers = [new SubmitAnswerBody(questionId, [optionId], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _submitAnswersUseCase.Execute(matchAttemptId, dto)
        );
    }

    [Fact]
    public async Task SubmitAnswer_MatchAttemptNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var matchAttemptId = Guid.NewGuid();

        _mockCurrentUser.Setup(user => user.UserId).Returns(userId);
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetByIdAsync(matchAttemptId))
            .ReturnsAsync((MatchAttempt?)null);

        var dto = new SubmitAnswersRequestDto
        {
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), [Guid.NewGuid()], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _submitAnswersUseCase.Execute(matchAttemptId, dto)
        );
    }

    [Fact]
    public async Task SubmitAnswer_EmptyAnswers_ThrowsValidationException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var matchAttemptId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        _mockCurrentUser.Setup(user => user.UserId).Returns(userId);
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetByIdAsync(matchAttemptId))
            .ReturnsAsync(new MatchAttempt { Id = matchAttemptId, MatchId = matchId, UserId = Guid.Parse(userId) });
        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new DomainMatch { Id = matchId, AttemptsAmount = 1 });
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetMatchAttemptCountByMatchIdAndUserIdAsync(matchId, Guid.Parse(userId)))
            .ReturnsAsync(1);

        var dto = new SubmitAnswersRequestDto { Answers = [] };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _submitAnswersUseCase.Execute(matchAttemptId, dto)
        );
    }

    [Fact]
    public async Task SubmitAnswer_AllAnswersWrong_ReturnsZeroScore()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var matchAttemptId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();

        _mockCurrentUser.Setup(user => user.UserId).Returns(userId);

        var matchAttempt = new MatchAttempt { Id = matchAttemptId, MatchId = matchId, UserId = Guid.Parse(userId) };
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetByIdAsync(matchAttemptId))
            .ReturnsAsync(matchAttempt);
        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new DomainMatch { Id = matchId, AttemptsAmount = 1 });
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetMatchAttemptCountByMatchIdAndUserIdAsync(matchId, Guid.Parse(userId)))
            .ReturnsAsync(1);

        var dto = new SubmitAnswersRequestDto
        {
            Answers = [new SubmitAnswerBody(questionId, [optionId], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        var answers = new List<Answer>
        {
            new() { QuestionId = questionId, SelectedOptions = [new SelectedOption { OptionId = optionId }] }
        };
        _mockMapper.Setup(m => m.Map<List<Answer>>(dto.Answers)).Returns(answers);

        _mockMatchAttemptRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync(matchAttempt);

        // The student picked a real option of the question, just the wrong one.
        var question = new Question
        {
            Id = questionId,
            Content = "Sample question",
            Type = QuestionType.SingleChoice,
            Options = [new Option { Id = Guid.NewGuid(), IsCorrect = true }, new Option { Id = optionId, IsCorrect = false }]
        };
        _mockQuestionRepository
            .Setup(repo => repo.GetByIdsWithOptionsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([question]);

        // Act
        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        // Assert
        Assert.Equal(0, result.ScorePercentage);
        Assert.Equal(0, result.CorrectCount);
        Assert.Equal(1, result.IncorrectCount);
        Assert.False(result.Questions[0].IsCorrect);
    }

    [Fact]
    public async Task SubmitAnswer_MatchNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var matchAttemptId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        _mockCurrentUser.Setup(user => user.UserId).Returns(userId);
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetByIdAsync(matchAttemptId))
            .ReturnsAsync(new MatchAttempt { Id = matchAttemptId, MatchId = matchId, UserId = Guid.Parse(userId) });
        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync((DomainMatch?)null);

        var dto = new SubmitAnswersRequestDto
        {
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), [Guid.NewGuid()], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _submitAnswersUseCase.Execute(matchAttemptId, dto)
        );
    }

    [Fact]
    public async Task SubmitAnswer_AttemptCountExceedsMatchLimit_ThrowsMaxAttemptsReachedException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var matchAttemptId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        _mockCurrentUser.Setup(user => user.UserId).Returns(userId);
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetByIdAsync(matchAttemptId))
            .ReturnsAsync(new MatchAttempt { Id = matchAttemptId, MatchId = matchId, UserId = Guid.Parse(userId) });
        _mockMatchRepository
            .Setup(repo => repo.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new DomainMatch { Id = matchId, AttemptsAmount = 1 });
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetMatchAttemptCountByMatchIdAndUserIdAsync(matchId, Guid.Parse(userId)))
            .ReturnsAsync(2);

        var dto = new SubmitAnswersRequestDto
        {
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), [Guid.NewGuid()], DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        // Act & Assert
        await Assert.ThrowsAsync<MaxAttemptsReachedException>(
            () => _submitAnswersUseCase.Execute(matchAttemptId, dto)
        );

        _mockMapper.Verify(mapper => mapper.Map<List<Answer>>(It.IsAny<object>()), Times.Never);
    }
}
