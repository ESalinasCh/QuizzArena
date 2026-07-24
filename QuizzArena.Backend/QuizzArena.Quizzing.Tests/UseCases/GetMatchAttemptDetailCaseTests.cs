using Moq;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchAttemptUseCases;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;


namespace QuizzArena.Quizzing.Tests.UseCases;

public class GetMatchAttemptDetailCaseTests
{
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;

    private readonly GetMatchAttemptDetail _getMatchAttemptDetail;

    public GetMatchAttemptDetailCaseTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();

        _getMatchAttemptDetail = new GetMatchAttemptDetail(
            _mockMatchRepository.Object,
            _mockQuestionRepository.Object
        );
    }

    [Fact]
    public async Task Execute_MatchAttemptNotFound_ThrowsInvalidOperationException()
    {
        Guid matchAttemptId = Guid.NewGuid();

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(matchAttemptId))
            .ReturnsAsync((MatchAttempt?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _getMatchAttemptDetail.Execute(matchAttemptId));
    }

    [Fact]
    public async Task Execute_ValidMatchAttempt_ReturnsMappedDetail()
    {
        Guid matchId = Guid.NewGuid();
        Guid matchAttemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId = Guid.NewGuid();

        var matchAttempt = new MatchAttempt
        {
            Id = matchAttemptId,
            Score = 100,
            Status = QuizAttemptStatus.Completed,
            MatchId = matchId,
            MatchAttemptQuestions =
            [
                new MatchAttemptQuestion
                {
                    QuestionId = questionId
                }
            ],
            Answers =
            [
                new Answer
                {
                    QuestionId = questionId,
                    SelectedOptions = [new SelectedOption { OptionId = optionId, IsCorrect = true }],
                    IsCorrect = true
                }
            ]
        };

        var match = new Domain.Entities.Match
        {
            Id = matchId,
            Status = MatchStatus.Active,
            Mode = MatchMode.Solo
        };

        var questions = new List<Question>
        {
            new()
            {
                Id = questionId,
                Content = "Question content",
                Options =
                [
                    new Option
                    {
                        Id = optionId,
                        Description = "Option A",
                        IsCorrect = true
                    }
                ]
            }
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(matchAttemptId))
            .ReturnsAsync(matchAttempt);

        _mockMatchRepository
           .Setup(x => x.GetMatchByIdAsync(matchId))
           .ReturnsAsync(match);

        _mockQuestionRepository
            .Setup(x => x.GetByIdsWithOptionsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(questions);

        GetMatchAttemptDetailDTO result =
            await _getMatchAttemptDetail.Execute(matchAttemptId);

        Assert.NotNull(result);
        Assert.Equal(matchAttemptId, result.Id);
        Assert.Equal(100, result.Score);
        Assert.Equal(QuizAttemptStatus.Completed, result.Status);

        Assert.Single(result.Questions);

        var question = result.Questions.Single();

        Assert.Equal(questionId, question.QuestionId);
        Assert.Equal("Question content", question.Content);
        Assert.Equal(optionId, Assert.Single(question.SelectedOptionIds));
        Assert.True(question.IsCorrect);

        Assert.Single(question.Options);

        var option = question.Options.Single();

        Assert.Equal(optionId, option.Id);
        Assert.Equal("Option A", option.Description);
        Assert.True(option.IsCorrect);
    }

    [Fact]
    public async Task Execute_QuestionWithoutAnswer_ReturnsDefaultValues()
    {
        Guid matchId = Guid.NewGuid();
        Guid matchAttemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        var match = new Domain.Entities.Match
        {
            Id = matchId,
            Status = MatchStatus.Active,
            Mode = MatchMode.Solo
        };

        var matchAttempt = new MatchAttempt
        {
            Id = matchAttemptId,
            Score = 50,
            Status = QuizAttemptStatus.Completed,
            MatchId = matchId,
            MatchAttemptQuestions =
            [
                new MatchAttemptQuestion
                {
                    QuestionId = questionId
                }
            ],
            Answers = []
        };

        var questions = new List<Question>
        {
            new()
            {
                Id = questionId,
                Content = "Question content",
                Options = []
            }
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(matchAttemptId))
            .ReturnsAsync(matchAttempt);

        _mockMatchRepository
          .Setup(x => x.GetMatchByIdAsync(matchId))
          .ReturnsAsync(match);

        _mockQuestionRepository
            .Setup(x => x.GetByIdsWithOptionsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(questions);

        GetMatchAttemptDetailDTO result =
            await _getMatchAttemptDetail.Execute(matchAttemptId);

        var question = result.Questions.Single();

        Assert.Empty(question.SelectedOptionIds);
        Assert.False(question.IsCorrect);
    }

    [Fact]
    public async Task Execute_MatchNotFound_ThrowsInvalidOperationException()
    {
        Guid matchId = Guid.NewGuid();
        Guid matchAttemptId = Guid.NewGuid();

        var matchAttempt = new MatchAttempt
        {
            Id = matchAttemptId,
            MatchId = matchId
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(matchAttemptId))
            .ReturnsAsync(matchAttempt);

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync((Domain.Entities.Match?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _getMatchAttemptDetail.Execute(matchAttemptId));
    }

    [Fact]
    public async Task Execute_ActiveExam_HidesResults()
    {
        Guid matchId = Guid.NewGuid();
        Guid matchAttemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid optionId = Guid.NewGuid();

        var match = new Domain.Entities.Match
        {
            Id = matchId,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-60),
            FinishedAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Mode = MatchMode.Exam
        };

        var matchAttempt = new MatchAttempt
        {
            Id = matchAttemptId,
            MatchId = matchId,
            Score = 100,
            Status = QuizAttemptStatus.Completed,
            MatchAttemptQuestions =
            [
                new MatchAttemptQuestion
                {
                    QuestionId = questionId
                }
            ],
            Answers =
            [
                new Answer
                {
                    QuestionId = questionId,
                    SelectedOptions = [new SelectedOption { OptionId = optionId, IsCorrect = true }],
                    IsCorrect = true
                }
            ]
        };

        var questions = new List<Question>
        {
            new()
            {
                Id = questionId,
                Content = "Question content",
                Justification = "Justification",
                Options =
                [
                    new Option
                    {
                        Id = optionId,
                        Description = "Option A",
                        IsCorrect = true
                    }
                ]
            }
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(matchAttemptId))
            .ReturnsAsync(matchAttempt);

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);

        _mockQuestionRepository
            .Setup(x => x.GetByIdsWithOptionsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(questions);

        var result = await _getMatchAttemptDetail.Execute(matchAttemptId);

        var question = result.Questions.Single();
        var option = question.Options.Single();

        Assert.Null(result.Score);
        Assert.Null(question.Justification);
        Assert.Null(question.IsCorrect);
        Assert.Null(option.IsCorrect);

        // The student's own picks are not a result, so hiding results must not hide them.
        Assert.Equal(optionId, Assert.Single(question.SelectedOptionIds));
    }

    [Fact]
    public async Task Execute_MultipleSelectedOptions_ReturnsEveryPick()
    {
        Guid matchId = Guid.NewGuid();
        Guid matchAttemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid firstOptionId = Guid.NewGuid();
        Guid secondOptionId = Guid.NewGuid();
        Guid unpickedOptionId = Guid.NewGuid();

        var match = new Domain.Entities.Match
        {
            Id = matchId,
            Status = MatchStatus.Active,
            Mode = MatchMode.Solo
        };

        var matchAttempt = new MatchAttempt
        {
            Id = matchAttemptId,
            Score = 100,
            Status = QuizAttemptStatus.Completed,
            MatchId = matchId,
            MatchAttemptQuestions = [new MatchAttemptQuestion { QuestionId = questionId }],
            Answers =
            [
                new Answer
                {
                    QuestionId = questionId,
                    SelectedOptions =
                    [
                        new SelectedOption { OptionId = firstOptionId, IsCorrect = true },
                        new SelectedOption { OptionId = secondOptionId, IsCorrect = true }
                    ],
                    IsCorrect = true
                }
            ]
        };

        var questions = new List<Question>
        {
            new()
            {
                Id = questionId,
                Content = "Question content",
                Type = QuestionType.MultipleChoice,
                Options =
                [
                    new Option { Id = firstOptionId, Description = "Option A", IsCorrect = true },
                    new Option { Id = secondOptionId, Description = "Option B", IsCorrect = true },
                    new Option { Id = unpickedOptionId, Description = "Option C", IsCorrect = false }
                ]
            }
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(matchAttemptId))
            .ReturnsAsync(matchAttempt);

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);

        _mockQuestionRepository
            .Setup(x => x.GetByIdsWithOptionsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(questions);

        GetMatchAttemptDetailDTO result = await _getMatchAttemptDetail.Execute(matchAttemptId);

        var question = result.Questions.Single();

        // Both picks survive: the old single OptionId could only ever carry one of them.
        Assert.Equal(2, question.SelectedOptionIds.Count);
        Assert.True(question.SelectedOptionIds.ToHashSet().SetEquals([firstOptionId, secondOptionId]));
        Assert.DoesNotContain(unpickedOptionId, question.SelectedOptionIds);
        Assert.True(question.IsCorrect);
    }

    [Fact]
    public async Task Execute_PartiallyCorrectMultipleChoice_ReportsPicksAndFailedVerdict()
    {
        Guid matchId = Guid.NewGuid();
        Guid matchAttemptId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();
        Guid correctOptionId = Guid.NewGuid();
        Guid missedOptionId = Guid.NewGuid();

        var match = new Domain.Entities.Match
        {
            Id = matchId,
            Status = MatchStatus.Active,
            Mode = MatchMode.Solo
        };

        // One of the two correct options picked: every pick is right, the answer is not.
        var matchAttempt = new MatchAttempt
        {
            Id = matchAttemptId,
            Score = 0,
            Status = QuizAttemptStatus.Completed,
            MatchId = matchId,
            MatchAttemptQuestions = [new MatchAttemptQuestion { QuestionId = questionId }],
            Answers =
            [
                new Answer
                {
                    QuestionId = questionId,
                    SelectedOptions = [new SelectedOption { OptionId = correctOptionId, IsCorrect = true }],
                    IsCorrect = false
                }
            ]
        };

        var questions = new List<Question>
        {
            new()
            {
                Id = questionId,
                Content = "Question content",
                Type = QuestionType.MultipleChoice,
                Options =
                [
                    new Option { Id = correctOptionId, Description = "Option A", IsCorrect = true },
                    new Option { Id = missedOptionId, Description = "Option B", IsCorrect = true }
                ]
            }
        };

        _mockMatchRepository
            .Setup(x => x.GetMatchAttemptsDetailById(matchAttemptId))
            .ReturnsAsync(matchAttempt);

        _mockMatchRepository
            .Setup(x => x.GetMatchByIdAsync(matchId))
            .ReturnsAsync(match);

        _mockQuestionRepository
            .Setup(x => x.GetByIdsWithOptionsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(questions);

        GetMatchAttemptDetailDTO result = await _getMatchAttemptDetail.Execute(matchAttemptId);

        var question = result.Questions.Single();

        Assert.Equal(correctOptionId, Assert.Single(question.SelectedOptionIds));

        // Reported straight from the stored verdict, not recomputed from the picks.
        Assert.False(question.IsCorrect);
    }
}
