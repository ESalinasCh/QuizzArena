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

    private readonly TrackAnswerUseCase _useCase;

    public TrackAnswerCaseTests()
    {
        _mockAnswerRepository = new Mock<IAnswerRepository>();
        _mockOptionRepository = new Mock<IOptionRepository>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();

        _useCase = new TrackAnswerUseCase(
            _mockAnswerRepository.Object,
            _mockOptionRepository.Object,
            _mockMatchRepository.Object,
            _mockCurrentUser.Object);
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
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = Guid.NewGuid()
            });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionId = Guid.NewGuid()
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
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = Guid.NewGuid()
            });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionId = Guid.NewGuid()
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
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync((MatchAttempt?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionId = Guid.NewGuid()
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
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
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
                    SelectedOptionId = Guid.NewGuid()
                }));
    }

    [Fact]
    public async Task Execute_OptionNotFound_ThrowsInvalidOperationException()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = userId,
                Status = QuizAttemptStatus.InProgress
            });

        _mockOptionRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Option?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionId = Guid.NewGuid()
                }));
    }

    [Fact]
    public async Task Execute_OptionDoesNotBelongToQuestion_ThrowsInvalidOperationException()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync(new MatchAttempt
            {
                UserId = userId,
                Status = QuizAttemptStatus.InProgress
            });

        _mockOptionRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Option
            {
                Id = Guid.NewGuid(),
                QuestionId = Guid.NewGuid(),
                IsCorrect = true
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(
                attemptId,
                questionId,
                new TrackAnswerRequestDto
                {
                    SelectedOptionId = Guid.NewGuid()
                }));
    }

    [Fact]
    public async Task Execute_NewAnswer_CreatesAnswerAndReturnsProgress()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId = Guid.NewGuid();

        MatchAttempt attempt = new()
        {
            UserId = userId,
            Status = QuizAttemptStatus.InProgress,
            Answers = new List<Answer>
            {
                new()
            },
            MatchAttemptQuestions = new List<MatchAttemptQuestion>
            {
                new(),
                new(),
                new()
            }
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockOptionRepository
            .Setup(x => x.GetByIdAsync(optionId))
            .ReturnsAsync(new Option
            {
                Id = optionId,
                QuestionId = questionId,
                IsCorrect = true
            });

        _mockAnswerRepository
            .Setup(x => x.GetByAttemptAndQuestionAsync(attemptId, questionId))
            .ReturnsAsync((Answer?)null);

        MatchAttemptSmallProgressDto result = await _useCase.Execute(
            attemptId,
            questionId,
            new TrackAnswerRequestDto
            {
                SelectedOptionId = optionId
            });

        _mockAnswerRepository.Verify(
            x => x.CreateAnswerAsync(It.Is<Answer>(a =>
                a.MatchAttemptId == attemptId &&
                a.QuestionId == questionId &&
                a.OptionId == optionId &&
                a.IsCorrect)),
            Times.Once);

        _mockAnswerRepository.Verify(
            x => x.UpdateAnswerAsync(It.IsAny<Answer>()),
            Times.Never);

        Assert.Equal(1, result.AnsweredQuestions);
        Assert.Equal(3, result.TotalQuestions);
    }

    [Fact]
    public async Task Execute_ExistingAnswer_UpdatesAnswerAndReturnsProgress()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId = Guid.NewGuid();

        Answer existingAnswer = new()
        {
            MatchAttemptId = attemptId,
            QuestionId = questionId
        };

        MatchAttempt attempt = new()
        {
            UserId = userId,
            Status = QuizAttemptStatus.InProgress,
            Answers = new List<Answer>
            {
                new(),
                new()
            },
            MatchAttemptQuestions = new List<MatchAttemptQuestion>
            {
                new(),
                new(),
                new(),
                new()
            }
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockOptionRepository
            .Setup(x => x.GetByIdAsync(optionId))
            .ReturnsAsync(new Option
            {
                Id = optionId,
                QuestionId = questionId,
                IsCorrect = false
            });

        _mockAnswerRepository
            .Setup(x => x.GetByAttemptAndQuestionAsync(attemptId, questionId))
            .ReturnsAsync(existingAnswer);

        MatchAttemptSmallProgressDto result = await _useCase.Execute(
            attemptId,
            questionId,
            new TrackAnswerRequestDto
            {
                SelectedOptionId = optionId
            });

        _mockAnswerRepository.Verify(
            x => x.UpdateAnswerAsync(It.Is<Answer>(a =>
                a.OptionId == optionId &&
                !a.IsCorrect)),
            Times.Once);

        _mockAnswerRepository.Verify(
            x => x.CreateAnswerAsync(It.IsAny<Answer>()),
            Times.Never);

        Assert.Equal(2, result.AnsweredQuestions);
        Assert.Equal(4, result.TotalQuestions);
    }
}
