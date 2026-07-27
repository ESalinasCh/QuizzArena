using Moq;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class TrackAnswerCaseTests
{
    private readonly Mock<IAnswerRepository> _mockAnswerRepository;
    private readonly Mock<IOptionRepository> _mockOptionRepository;
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;


    private readonly TrackAnswerUseCase _useCase;

    public TrackAnswerCaseTests()
    {
        _mockAnswerRepository = new Mock<IAnswerRepository>();
        _mockOptionRepository = new Mock<IOptionRepository>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();

        _useCase = new TrackAnswerUseCase(
            _mockAnswerRepository.Object,
            _mockOptionRepository.Object,
            _mockMatchRepository.Object,
            _mockQuestionRepository.Object,
            _mockCurrentUser.Object
            );
    }

    [Fact]
    public async Task Execute_InvalidUserId_ThrowsUnauthorizedAccessException()
    {
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns("invalid-guid");

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = Guid.NewGuid()
            });

        _mockQuestionRepository.Setup(x => x.GetByIdWithOptionsAsync(questionId)).ReturnsAsync(new Question());


        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionIds = new List<Guid> { Guid.NewGuid() }
                }));
    }

    [Fact]
    public async Task Execute_AttemptBelongsToAnotherUser_ThrowsUnauthorizedAccessException()
    {
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = Guid.NewGuid()
            });
        _mockQuestionRepository.Setup(x => x.GetByIdWithOptionsAsync(questionId)).ReturnsAsync(new Question());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionIds = new List<Guid> { Guid.NewGuid() }
                }));
    }

    [Fact]
    public async Task Execute_AttemptNotFound_ThrowsInvalidOperationException()
    {
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync((MatchAttempt?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionIds = new List<Guid> { Guid.NewGuid() }
                }));
    }

    [Fact]
    public async Task Execute_AttemptNotInProgress_ThrowsInvalidOperationException()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = userId,
                Status = QuizAttemptStatus.Completed
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionIds = new List<Guid> { Guid.NewGuid() }
                }));
    }

    [Fact]
    public async Task Execute_QuestionDoesNotBelongToAttempt_ThrowsInvalidOperationException()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = userId,
                Status = QuizAttemptStatus.InProgress,
                MatchAttemptQuestions = new List<MatchAttemptQuestion>
                {
                new() { QuestionId = Guid.NewGuid() }
                }
            });

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(new Question
            {
                Id = questionId,
                Options = new List<Option>
                {
                new() { Id = optionId }
                }
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionIds = new List<Guid> { optionId }
                }));
    }

    [Fact]
    public async Task Execute_QuestionNotFound_ThrowsInvalidOperationException()
    {
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = Guid.NewGuid(),
                Status = QuizAttemptStatus.InProgress
            });

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync((Question?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionIds = new List<Guid> { Guid.NewGuid() }
                }));
    }

    [Fact]
    public async Task Execute_OptionsNotAllowedOrDoNotBelongToQuestion_ThrowsInvalidOperationException()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid validOptionId = Guid.NewGuid();
        Guid invalidOptionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = userId,
                Status = QuizAttemptStatus.InProgress,
                MatchAttemptQuestions = new List<MatchAttemptQuestion>
                {
                new() { QuestionId = questionId }
                }
            });

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(new Question
            {
                Id = questionId,
                Type = QuestionType.MultipleChoice,
                Options = new List<Option>
                {
                new() { Id = validOptionId }
                }
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionIds = new List<Guid> { validOptionId, invalidOptionId }
                }));
    }

    [Fact]
    public async Task Execute_SingleChoiceCorrectAnswer_CreatesAnswerAndReturnsProgress()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId = Guid.NewGuid();

        MatchAttempt attempt = new()
        {
            UserId = userId,
            Status = QuizAttemptStatus.InProgress,
            Answers = new List<Answer>(),
            MatchAttemptQuestions = new List<MatchAttemptQuestion>
        {
            new() { QuestionId = questionId },
            new() { QuestionId = Guid.NewGuid() },
            new() { QuestionId = Guid.NewGuid() }
        }
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(new Question
            {
                Id = questionId,
                Type = QuestionType.SingleChoice,
                Options = new List<Option>
                {
                new() { Id = optionId, IsCorrect = true }
                }
            });

        _mockAnswerRepository
            .Setup(x => x.GetByAttemptAndQuestionAsync(attemptId, questionId))
            .ReturnsAsync((Answer?)null);

        MatchAttemptSmallProgressDto result = await _useCase.Execute(
            attemptId,
            questionId,
            new TrackAnswerRequestDto
            {
                SelectedOptionIds = new List<Guid> { optionId }
            });

        _mockAnswerRepository.Verify(
            x => x.CreateAnswerAsync(It.Is<Answer>(a =>
                a.MatchAttemptId == attemptId &&
                a.QuestionId == questionId &&
                a.IsCorrect &&
                a.SelectedOptions.Count == 1 &&
                a.SelectedOptions.Any(so => so.OptionId == optionId && so.IsCorrect))),
            Times.Once);

        _mockAnswerRepository.Verify(
            x => x.UpdateAnswerAndReplaceOptionsAsync(It.IsAny<Answer>(), It.IsAny<List<SelectedOption>>()),
            Times.Never);

        Assert.Equal(0, result.AnsweredQuestions);
        Assert.Equal(3, result.TotalQuestions);
    }

    [Fact]
    public async Task Execute_MultipleChoiceAllCorrect_CreatesAnswerAndReturnsProgress()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId1 = Guid.NewGuid();
        Guid optionId2 = Guid.NewGuid();

        MatchAttempt attempt = new()
        {
            UserId = userId,
            Status = QuizAttemptStatus.InProgress,
            Answers = new List<Answer>(),
            MatchAttemptQuestions = new List<MatchAttemptQuestion>
        {
            new() { QuestionId = questionId },
            new() { QuestionId = Guid.NewGuid() },
            new() { QuestionId = Guid.NewGuid() }
        }
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(new Question
            {
                Id = questionId,
                Type = QuestionType.MultipleChoice,
                Options = new List<Option>
                {
                new() { Id = optionId1, IsCorrect = true },
                new() { Id = optionId2, IsCorrect = true }
                }
            });

        _mockAnswerRepository
            .Setup(x => x.GetByAttemptAndQuestionAsync(attemptId, questionId))
            .ReturnsAsync((Answer?)null);

        MatchAttemptSmallProgressDto result = await _useCase.Execute(
            attemptId,
            questionId,
            new TrackAnswerRequestDto
            {
                SelectedOptionIds = new List<Guid> { optionId1, optionId2 }
            });

        _mockAnswerRepository.Verify(
            x => x.CreateAnswerAsync(It.Is<Answer>(a =>
                a.MatchAttemptId == attemptId &&
                a.QuestionId == questionId &&
                a.IsCorrect &&
                a.SelectedOptions.Count == 2 &&
                a.SelectedOptions.All(so => so.IsCorrect))),
            Times.Once);

        _mockAnswerRepository.Verify(
            x => x.UpdateAnswerAndReplaceOptionsAsync(It.IsAny<Answer>(), It.IsAny<List<SelectedOption>>()),
            Times.Never);

        Assert.Equal(0, result.AnsweredQuestions);
        Assert.Equal(3, result.TotalQuestions);
    }

    [Fact]
    public async Task Execute_MultipleChoicePartialCorrect_CreatesAnswerAndReturnsProgress()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid correctOptionId = Guid.NewGuid();
        Guid incorrectOptionId = Guid.NewGuid();

        MatchAttempt attempt = new()
        {
            UserId = userId,
            Status = QuizAttemptStatus.InProgress,
            Answers = new List<Answer>(),
            MatchAttemptQuestions = new List<MatchAttemptQuestion>
        {
            new() { QuestionId = questionId },
            new() { QuestionId = Guid.NewGuid() },
            new() { QuestionId = Guid.NewGuid() }
        }
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(new Question
            {
                Id = questionId,
                Type = QuestionType.MultipleChoice,
                Options = new List<Option>
                {
                new() { Id = correctOptionId, IsCorrect = true },
                new() { Id = incorrectOptionId, IsCorrect = false }
                }
            });

        _mockAnswerRepository
            .Setup(x => x.GetByAttemptAndQuestionAsync(attemptId, questionId))
            .ReturnsAsync((Answer?)null);

        MatchAttemptSmallProgressDto result = await _useCase.Execute(
            attemptId,
            questionId,
            new TrackAnswerRequestDto
            {
                SelectedOptionIds = new List<Guid> { correctOptionId, incorrectOptionId }
            });

        _mockAnswerRepository.Verify(
            x => x.CreateAnswerAsync(It.Is<Answer>(a =>
                a.MatchAttemptId == attemptId &&
                a.QuestionId == questionId &&
                !a.IsCorrect &&
                a.SelectedOptions.Count == 2)),
            Times.Once);

        Assert.Equal(0, result.AnsweredQuestions);
        Assert.Equal(3, result.TotalQuestions);
    }

    [Fact]
    public async Task Execute_ExistingAnswer_UpdatesAnswerAndReturnsProgress()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId1 = Guid.NewGuid();
        Guid optionId2 = Guid.NewGuid();

        Answer existingAnswer = new()
        {
            MatchAttemptId = attemptId,
            QuestionId = questionId,
            SelectedOptions = new List<SelectedOption>
        {
            new() { OptionId = Guid.NewGuid() }
        }
        };

        MatchAttempt attempt = new()
        {
            UserId = userId,
            Status = QuizAttemptStatus.InProgress,
            Answers = new List<Answer>
        {
            existingAnswer,
            new Answer()
        },
            MatchAttemptQuestions = new List<MatchAttemptQuestion>
        {
            new() { QuestionId = questionId },
            new() { QuestionId = Guid.NewGuid() },
            new() { QuestionId = Guid.NewGuid() },
            new() { QuestionId = Guid.NewGuid() }
        }
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(new Question
            {
                Id = questionId,
                Type = QuestionType.MultipleChoice,
                Options = new List<Option>
                {
                new() { Id = optionId1, IsCorrect = true },
                new() { Id = optionId2, IsCorrect = false }
                }
            });

        _mockAnswerRepository
            .Setup(x => x.GetByAttemptAndQuestionAsync(attemptId, questionId))
            .ReturnsAsync(existingAnswer);

        MatchAttemptSmallProgressDto result = await _useCase.Execute(
            attemptId,
            questionId,
            new TrackAnswerRequestDto
            {
                SelectedOptionIds = new List<Guid> { optionId1 }
            });

        _mockAnswerRepository.Verify(
    x => x.UpdateAnswerAndReplaceOptionsAsync(
        It.Is<Answer>(a =>
            a.MatchAttemptId == attemptId &&
            a.QuestionId == questionId &&
            a.IsCorrect),
        It.Is<List<SelectedOption>>(l =>
            l.Count == 1 &&
            l[0].OptionId == optionId1 &&
            l[0].IsCorrect)),
    Times.Once);

        _mockAnswerRepository.Verify(
            x => x.CreateAnswerAsync(It.IsAny<Answer>()),
            Times.Never);

        Assert.Equal(2, result.AnsweredQuestions);
        Assert.Equal(4, result.TotalQuestions);
    }
}
