
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
            Answers = [new SubmitAnswerBody(questionId, optionId, DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        var answers = new List<Answer> { new() { QuestionId = questionId, OptionId = optionId } };
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
    /// </summary>
    private (Guid MatchAttemptId, SubmitAnswersRequestDto Dto) ArrangeSingleQuestion(
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

        // One request entry per selected option — this is how a client expresses
        // a multi-select answer without a schema change.
        var dto = new SubmitAnswersRequestDto
        {
            Answers = [.. selectedOptionIds.Select(
                id => new SubmitAnswerBody(questionId, id, DateTimeOffset.UtcNow.AddMinutes(-1)))]
        };

        List<Answer> answers = [.. selectedOptionIds.Select(
            id => new Answer { QuestionId = questionId, OptionId = id })];
        _mockMapper.Setup(m => m.Map<List<Answer>>(dto.Answers)).Returns(answers);

        var question = new Question { Id = questionId, Content = "Sample question", Type = type, Options = options };
        _mockQuestionRepository
            .Setup(repo => repo.GetByIdsWithOptionsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([question]);

        return (matchAttemptId, dto);
    }

    [Fact]
    public async Task SubmitAnswer_MultipleChoiceAllCorrectOptionsSelected_ScoresCorrect()
    {
        Option a = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option b = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option c = new() { Id = Guid.NewGuid(), IsCorrect = false };

        (Guid matchAttemptId, SubmitAnswersRequestDto dto) = ArrangeSingleQuestion(
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
        (Guid matchAttemptId, SubmitAnswersRequestDto dto) = ArrangeSingleQuestion(
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
        (Guid matchAttemptId, SubmitAnswersRequestDto dto) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, [a, b, c], [a.Id, b.Id, c.Id]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(0, result.ScorePercentage);
        Assert.False(result.Questions[0].IsCorrect);
    }

    [Fact]
    public async Task SubmitAnswer_SingleChoiceMultipleOptionsSelected_ScoresIncorrect()
    {
        Option correct = new() { Id = Guid.NewGuid(), IsCorrect = true };
        Option wrong = new() { Id = Guid.NewGuid(), IsCorrect = false };

        // Hedging by picking both must not count as correct on a single-choice question.
        (Guid matchAttemptId, SubmitAnswersRequestDto dto) = ArrangeSingleQuestion(
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

        (Guid matchAttemptId, SubmitAnswersRequestDto dto) = ArrangeSingleQuestion(
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

        // Three request entries, one question: the score denominator must be 1, not 3.
        (Guid matchAttemptId, SubmitAnswersRequestDto dto) = ArrangeSingleQuestion(
            QuestionType.MultipleChoice, [a, b, c], [a.Id, b.Id, c.Id]);

        SubmitAnswersResponseDto result = await _submitAnswersUseCase.Execute(matchAttemptId, dto);

        Assert.Equal(1, result.TotalQuestions);
        Assert.Equal(1, result.CorrectCount);
        Assert.Equal(0, result.IncorrectCount);
        Assert.Equal(100, result.ScorePercentage);
        Assert.Single(result.Questions);
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
            Answers = [new SubmitAnswerBody(questionId, optionId, DateTimeOffset.UtcNow.AddMinutes(-1))]
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
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-1))]
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
            Answers = [new SubmitAnswerBody(questionId, optionId, DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        var answers = new List<Answer> { new() { QuestionId = questionId, OptionId = optionId } };
        _mockMapper.Setup(m => m.Map<List<Answer>>(dto.Answers)).Returns(answers);

        _mockMatchAttemptRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync(matchAttempt);

        var question = new Question { Id = questionId, Content = "Sample question", Type = QuestionType.SingleChoice, Options = [new Option { Id = Guid.NewGuid(), IsCorrect = true }] };
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
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-1))]
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
            Answers = [new SubmitAnswerBody(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        // Act & Assert
        await Assert.ThrowsAsync<MaxAttemptsReachedException>(
            () => _submitAnswersUseCase.Execute(matchAttemptId, dto)
        );

        _mockMapper.Verify(mapper => mapper.Map<List<Answer>>(It.IsAny<object>()), Times.Never);
    }
}
