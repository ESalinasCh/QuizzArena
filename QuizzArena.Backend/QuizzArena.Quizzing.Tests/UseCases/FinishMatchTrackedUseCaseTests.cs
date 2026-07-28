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
    public async Task Execute_ValidAttemptWithSingleChoice_ReturnsFinishedMatch()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid matchId = Guid.NewGuid();
        Guid questionId1 = Guid.NewGuid();
        Guid questionId2 = Guid.NewGuid();
        Guid optionId1 = Guid.NewGuid();
        Guid optionId2 = Guid.NewGuid();

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
                    QuestionId = questionId1,
                    ValueScore = 100
                },
                new MatchAttemptQuestion
                {
                    QuestionId = questionId2,
                    ValueScore = 100
                }
            ],
            Answers =
            [
                new Answer
                {
                    QuestionId = questionId1,
                    IsCorrect = true,
                    SelectedOptions = new List<SelectedOption>
                    {
                        new() { OptionId = optionId1, IsCorrect = true }
                    }
                },
                new Answer
                {
                    QuestionId = questionId2,
                    IsCorrect = false,
                    SelectedOptions = new List<SelectedOption>
                    {
                        new() { OptionId = optionId2, IsCorrect = false }
                    }
                }
            ]
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
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
                    Id = questionId1,
                    Content = "Question 1"
                },
                new Question
                {
                    Id = questionId2,
                    Content = "Question 2"
                }
            ]);

        FinishedMatchTrackedDto result = await _useCase.Execute(attemptId);

        Assert.NotNull(result);
        Assert.Equal(attemptId, result.AttemptId);
        Assert.Equal(2, result.AnsweredQuestions);
        Assert.Equal(2, result.TotalQuestions);
        Assert.Equal(2, result.Answers.Count);

        Assert.Equal(QuizAttemptStatus.Completed, attempt.Status);
        Assert.Equal(100, attempt.Score);
        Assert.NotNull(attempt.EndDateTime);

        _mockMatchAttemptRepository.Verify(
            x => x.UpdateAsync(It.IsAny<MatchAttempt>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_ValidAttemptWithMultipleChoice_ReturnsFinishedMatch()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid matchId = Guid.NewGuid();
        Guid questionId1 = Guid.NewGuid();
        Guid questionId2 = Guid.NewGuid();
        Guid optionId1 = Guid.NewGuid();
        Guid optionId2 = Guid.NewGuid();
        Guid optionId3 = Guid.NewGuid();
        Guid optionId4 = Guid.NewGuid();

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
                    QuestionId = questionId1,
                    ValueScore = 100
                },
                new MatchAttemptQuestion
                {
                    QuestionId = questionId2,
                    ValueScore = 100
                }
            ],
            Answers =
            [
                new Answer
                {
                    QuestionId = questionId1,
                    IsCorrect = true,
                    SelectedOptions = new List<SelectedOption>
                    {
                        new() { OptionId = optionId1, IsCorrect = true },
                        new() { OptionId = optionId2, IsCorrect = true }
                    }
                },
                new Answer
                {
                    QuestionId = questionId2,
                    IsCorrect = false,
                    SelectedOptions = new List<SelectedOption>
                    {
                        new() { OptionId = optionId3, IsCorrect = true },
                        new() { OptionId = optionId4, IsCorrect = false }
                    }
                }
            ]
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
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
                    Id = questionId1,
                    Content = "Question 1"
                },
                new Question
                {
                    Id = questionId2,
                    Content = "Question 2"
                }
            ]);

        FinishedMatchTrackedDto result = await _useCase.Execute(attemptId);

        Assert.NotNull(result);
        Assert.Equal(attemptId, result.AttemptId);
        Assert.Equal(2, result.AnsweredQuestions);
        Assert.Equal(2, result.TotalQuestions);
        Assert.Equal(2, result.Answers.Count);

        var answer1 = result.Answers.First(a => a.Id == questionId1);
        Assert.Equal(2, answer1.SelectedOptionIds.Count);
        Assert.Contains(optionId1, answer1.SelectedOptionIds);
        Assert.Contains(optionId2, answer1.SelectedOptionIds);

        var answer2 = result.Answers.First(a => a.Id == questionId2);
        Assert.Equal(2, answer2.SelectedOptionIds.Count);
        Assert.Contains(optionId3, answer2.SelectedOptionIds);
        Assert.Contains(optionId4, answer2.SelectedOptionIds);

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

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _useCase.Execute(Guid.NewGuid()));

        Assert.Equal("Invalid user identity.", exception.Message);
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
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
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
            .Setup(x => x.GetMatchAttemptDetailById(It.IsAny<Guid>()))
            .ReturnsAsync(attempt);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _useCase.Execute(Guid.NewGuid()));

        Assert.Equal("User doesn't belong to this match attempt.", exception.Message);
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
            .Setup(x => x.GetMatchAttemptDetailById(It.IsAny<Guid>()))
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
            MatchId = matchId,
            Status = QuizAttemptStatus.InProgress
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
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
    public async Task Execute_MatchNotFound_ThrowsInvalidOperationException()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid matchId = Guid.NewGuid();

        var attempt = new MatchAttempt
        {
            Id = attemptId,
            UserId = userId,
            MatchId = matchId,
            Status = QuizAttemptStatus.InProgress
        };

        _mockCurrentUser
            .Setup(x => x.UserId)
            .Returns(userId.ToString());

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
            .ReturnsAsync(attempt);

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync((Match?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.Execute(attemptId));

        Assert.Equal($"Match not found for {matchId}.", exception.Message);
    }

    [Fact]
    public async Task Execute_QuestionWithoutAnswer_ReturnsEmptySelectedOptionIds()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid matchId = Guid.NewGuid();
        Guid questionId1 = Guid.NewGuid();
        Guid questionId2 = Guid.NewGuid();

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
                    QuestionId = questionId1,
                    ValueScore = 100
                },
                new MatchAttemptQuestion
                {
                    QuestionId = questionId2,
                    ValueScore = 100
                }
            ],
            Answers =
            [
                new Answer
                {
                    QuestionId = questionId1,
                    IsCorrect = true,
                    SelectedOptions = new List<SelectedOption>
                    {
                        new() { OptionId = Guid.NewGuid(), IsCorrect = true }
                    }
                }
            ]
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
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
                    Id = questionId1,
                    Content = "Question 1"
                },
                new Question
                {
                    Id = questionId2,
                    Content = "Question 2"
                }
            ]);

        FinishedMatchTrackedDto result = await _useCase.Execute(attemptId);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalQuestions);
        Assert.Equal(1, result.AnsweredQuestions);

        var answeredQuestion = result.Answers.First(a => a.Id == questionId1);
        Assert.Single(answeredQuestion.SelectedOptionIds);

        var unansweredQuestion = result.Answers.First(a => a.Id == questionId2);
        Assert.Empty(unansweredQuestion.SelectedOptionIds);

        Assert.Equal(QuizAttemptStatus.Completed, attempt.Status);
        Assert.Equal(100, attempt.Score);
        Assert.NotNull(attempt.EndDateTime);

        _mockMatchAttemptRepository.Verify(
            x => x.UpdateAsync(It.IsAny<MatchAttempt>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_ScoreCalculationWithDifferentValueScores_ReturnsCorrectScore()
    {
        Guid userId = Guid.NewGuid();
        Guid attemptId = Guid.NewGuid();
        Guid matchId = Guid.NewGuid();
        Guid questionId1 = Guid.NewGuid();
        Guid questionId2 = Guid.NewGuid();
        Guid questionId3 = Guid.NewGuid();

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
                    QuestionId = questionId1,
                    ValueScore = 50
                },
                new MatchAttemptQuestion
                {
                    QuestionId = questionId2,
                    ValueScore = 30
                },
                new MatchAttemptQuestion
                {
                    QuestionId = questionId3,
                    ValueScore = null
                }
            ],
            Answers =
            [
                new Answer
                {
                    QuestionId = questionId1,
                    IsCorrect = true,
                    SelectedOptions = new List<SelectedOption>()
                },
                new Answer
                {
                    QuestionId = questionId2,
                    IsCorrect = false,
                    SelectedOptions = new List<SelectedOption>()
                },
                new Answer
                {
                    QuestionId = questionId3,
                    IsCorrect = true,
                    SelectedOptions = new List<SelectedOption>()
                }
            ]
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptDetailById(attemptId))
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
                new Question { Id = questionId1, Content = "Question 1" },
                new Question { Id = questionId2, Content = "Question 2" },
                new Question { Id = questionId3, Content = "Question 3" }
            ]);

        FinishedMatchTrackedDto result = await _useCase.Execute(attemptId);

        decimal expectedScore = 50 + 0 + (100m / 3);
        Assert.Equal(Math.Round(expectedScore, 2), Math.Round(attempt.Score, 2));
        Assert.Equal(QuizAttemptStatus.Completed, attempt.Status);
        Assert.NotNull(attempt.EndDateTime);

        _mockMatchAttemptRepository.Verify(
            x => x.UpdateAsync(It.IsAny<MatchAttempt>()),
            Times.Once);
    }
}
