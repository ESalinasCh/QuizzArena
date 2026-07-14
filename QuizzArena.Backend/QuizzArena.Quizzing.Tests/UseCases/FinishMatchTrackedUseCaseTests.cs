using Moq;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Domain.Exceptions;
using Shared.Contracts;
using Match = QuizzArena.Quizzing.Domain.Entities.Match;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class FinishMatchTrackedUseCaseTests
{
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IMatchAttemptRepository> _mockMatchAttemptRepository;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<ICurrentUser> _mockCurrentUser;

    private readonly FinishMatchTrackedUseCase _useCase;

    public FinishMatchTrackedUseCaseTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockMatchAttemptRepository = new Mock<IMatchAttemptRepository>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();

        _useCase = new FinishMatchTrackedUseCase(
            _mockMatchRepository.Object,
            _mockMatchAttemptRepository.Object,
            _mockQuestionRepository.Object,
            _mockCurrentUser.Object
        );
    }

    [Fact]
    public async Task Execute_ValidAttempt_ReturnsFinishedMatch()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid matchId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        var attempt = new MatchAttempt
        {
            Id = attemptId,
            UserId = userId,
            MatchId = matchId,
            Status = QuizAttemptStatus.InProgress,
            MatchAttemptQuestions =
            [
                new MatchAttemptQuestion
            {
                QuestionId = questionId,
                ValueScore = 100
            }
            ],
            Answers =
            [
                new Answer
            {
                QuestionId = questionId,
                OptionId = optionId,
                IsCorrect = true
            }
            ]
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Match
            {
                Id = matchId,
                AttemptsAmount = 1
            });

        _mockMatchAttemptRepository
            .Setup(x => x.GetMatchAttemptCountByMatchIdAndUserIdAsync(matchId, userId))
            .ReturnsAsync(1);

        _mockQuestionRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(
            [
                new Question
            {
                Id = questionId,
                Content = "Question 1"
            }
            ]);

        FinishedMatchTrackedDto result = await _useCase.Execute(attemptId);

        Assert.NotNull(result);
        Assert.Equal(attemptId, result.AttemptId);
        Assert.Equal(1, result.AnsweredQuestions);
        Assert.Equal(1, result.TotalQuestions);
        Assert.Single(result.Answers);

        Assert.Equal(QuizAttemptStatus.Completed, attempt.Status);
        Assert.Equal(100, attempt.Score);
        Assert.NotNull(attempt.EndDateTime);

        _mockMatchAttemptRepository.Verify(
            x => x.UpdateAsync(It.IsAny<MatchAttempt>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_InvalidUserId_ThrowsUnauthorizedAccessException()
    {
        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns("invalid-user");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _useCase.Execute(Guid.NewGuid()));
    }

    [Fact]
    public async Task Execute_AttemptNotFound_ThrowsInvalidOperationException()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync((MatchAttempt?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.Execute(attemptId));
    }

    [Fact]
    public async Task Execute_AttemptBelongsToAnotherUser_ThrowsUnauthorizedAccessException()
    {
        Guid userId = Guid.NewGuid();

        var attempt = new MatchAttempt
        {
            UserId = Guid.NewGuid()
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(It.IsAny<Guid>()))
            .ReturnsAsync(attempt);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _useCase.Execute(Guid.NewGuid()));
    }

    [Fact]
    public async Task Execute_AttemptAlreadyCompleted_ThrowsAttemptAlreadyCompletedException()
    {
        Guid userId = Guid.NewGuid();

        var attempt = new MatchAttempt
        {
            UserId = userId,
            Status = QuizAttemptStatus.Completed
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(It.IsAny<Guid>()))
            .ReturnsAsync(attempt);

        await Assert.ThrowsAsync<AttemptAlreadyCompletedException>(
            () => _useCase.Execute(Guid.NewGuid()));
    }

    [Fact]
    public async Task Execute_MaximumAttemptsReached_ThrowsMaxAttemptsReachedException()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid matchId = Guid.NewGuid();

        var attempt = new MatchAttempt
        {
            Id = attemptId,
            UserId = userId,
            MatchId = matchId
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Match
            {
                Id = matchId,
                AttemptsAmount = 1
            });

        _mockMatchAttemptRepository
            .Setup(x => x.GetMatchAttemptCountByMatchIdAndUserIdAsync(matchId, userId))
            .ReturnsAsync(2);

        await Assert.ThrowsAsync<MaxAttemptsReachedException>(
            () => _useCase.Execute(attemptId));
    }

    [Fact]
    public async Task Execute_QuestionWithoutAnswer_ReturnsNullSelectedOptionId()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid matchId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        var attempt = new MatchAttempt
        {
            Id = attemptId,
            UserId = userId,
            MatchId = matchId,
            MatchAttemptQuestions =
            [
                new MatchAttemptQuestion
            {
                QuestionId = questionId,
                ValueScore = 100
            }
            ]
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(new Match
            {
                Id = matchId,
                AttemptsAmount = 1
            });

        _mockMatchAttemptRepository
            .Setup(x => x.GetMatchAttemptCountByMatchIdAndUserIdAsync(matchId, userId))
            .ReturnsAsync(1);

        _mockQuestionRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(
            [
                new Question
            {
                Id = questionId,
                Content = "Question"
            }
            ]);

        FinishedMatchTrackedDto result = await _useCase.Execute(attemptId);

        Assert.Null(result.Answers.Single().SelectedOptionId);
    }
}
