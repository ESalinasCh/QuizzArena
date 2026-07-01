using Moq;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchUseCases;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class StartAttemptUseCaseTests
{
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Mock<ICourseContract> _mockCourseImpl;
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IQuizRepository> _mockQuizRepository;
    private readonly Mock<IQuizQuestionRepository> _mockQuizQuestionRepository;
    private readonly Mock<IMatchAttemptRepository> _mockMatchAttemptRepository;

    private readonly StartAttemptUseCase _useCase;

    public StartAttemptUseCaseTests()
    {
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockCourseImpl = new Mock<ICourseContract>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockQuizRepository = new Mock<IQuizRepository>();
        _mockQuizQuestionRepository = new Mock<IQuizQuestionRepository>();
        _mockMatchAttemptRepository = new Mock<IMatchAttemptRepository>();

        _useCase = new StartAttemptUseCase(
            _mockCurrentUser.Object,
            _mockCourseImpl.Object,
            _mockMatchRepository.Object,
            _mockQuizRepository.Object,
            _mockQuizQuestionRepository.Object,
            _mockMatchAttemptRepository.Object
        );
    }

    [Fact]
    public async Task Execute_InvalidUserId_ThrowsFormatException()
    {
        // Arrange
        _mockCurrentUser.Setup(c => c.UserId).Returns("not-a-guid");
        var request = new StartAttemptRequestDto { MatchId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _useCase.Execute(request));
    }

    [Fact]
    public async Task Execute_MatchNotFoundOrUserNotEnrolled_ThrowsException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(new List<CourseSummaryDTO>());

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = Guid.NewGuid() };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(request));
        Assert.Equal("Match not found or user not enrolled in the course.", ex.Message);
    }

    [Fact]
    public async Task Execute_MatchNotActive_ThrowsException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Pending };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(request));
        Assert.Equal("Match is not active.", ex.Message);
    }

    [Fact]
    public async Task Execute_MaximumAttemptsReached_ThrowsException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Active, AttemptsAmount = 1 };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);
        _mockMatchAttemptRepository.Setup(r => r.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, Guid.Parse(userId))).ReturnsAsync(1);

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(request));
        Assert.Equal("Maximum number of attempts reached for this match.", ex.Message);
    }

    [Fact]
    public async Task Execute_UserHasActiveAttempt_ThrowsException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Active, AttemptsAmount = 2 };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);
        _mockMatchAttemptRepository.Setup(r => r.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, Guid.Parse(userId))).ReturnsAsync(0);
        _mockMatchAttemptRepository.Setup(r => r.HasActiveAttemptByMatchIdAsync(match.Id, Guid.Parse(userId))).ReturnsAsync(true);

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(request));
        Assert.Equal("User already have an active attempt for this match.", ex.Message);
    }

    [Fact]
    public async Task Execute_QuizNotFound_ThrowsException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Active, AttemptsAmount = 2, QuizId = Guid.NewGuid() };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);
        _mockMatchAttemptRepository.Setup(r => r.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, Guid.Parse(userId))).ReturnsAsync(0);
        _mockMatchAttemptRepository.Setup(r => r.HasActiveAttemptByMatchIdAsync(match.Id, Guid.Parse(userId))).ReturnsAsync(false);
        _mockQuizRepository.Setup(q => q.GetByIdAsync(match.QuizId)).ReturnsAsync((Quiz?)null);

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(request));
        Assert.Equal("No quiz and questions were found for this match.", ex.Message);
    }

    [Fact]
    public async Task Execute_NoQuestionsFound_ThrowsException()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(Guid.Parse(userId))).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Active, AttemptsAmount = 2, QuizId = Guid.NewGuid() };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);
        _mockMatchAttemptRepository.Setup(r => r.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, Guid.Parse(userId))).ReturnsAsync(0);
        _mockMatchAttemptRepository.Setup(r => r.HasActiveAttemptByMatchIdAsync(match.Id, Guid.Parse(userId))).ReturnsAsync(false);
        _mockQuizRepository.Setup(q => q.GetByIdAsync(match.QuizId)).ReturnsAsync(new Quiz { Id = match.QuizId });
        _mockQuizQuestionRepository.Setup(q => q.GetQuestionsAndScoreByQuizIdAsync(match.QuizId)).ReturnsAsync(new List<AugmentedQuestionDto>());

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(request));

        // Assert
        Assert.Equal("No questions were found for this match", ex.Message);
    }

    [Fact]
    public async Task Execute_Success_ReturnsMappedResponse()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var userGuid = Guid.Parse(userId);
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(userGuid)).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Active, AttemptsAmount = 2, QuizId = Guid.NewGuid(), ShuffleOptions = false, ShuffleQuestion = false };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);
        _mockMatchAttemptRepository.Setup(r => r.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, userGuid)).ReturnsAsync(0);
        _mockMatchAttemptRepository.Setup(r => r.HasActiveAttemptByMatchIdAsync(match.Id, userGuid)).ReturnsAsync(false);

        var quiz = new Quiz { Id = match.QuizId };
        _mockQuizRepository.Setup(q => q.GetByIdAsync(match.QuizId)).ReturnsAsync(quiz);

        var question1 = new Question
        {
            Id = Guid.NewGuid(),
            Content = "Q1",
            Options = new List<Option> {
                new Option { Id = Guid.NewGuid(), Description = "A" },
                new Option { Id = Guid.NewGuid(), Description = "B" }
            }
        };

        _mockQuizQuestionRepository.Setup(q => q.GetQuestionsAndScoreByQuizIdAsync(quiz.Id))
            .ReturnsAsync(new List<AugmentedQuestionDto> { new AugmentedQuestionDto(question1, 1m) });

        _mockMatchAttemptRepository.Setup(r => r.AddMatchAttemptAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync((MatchAttempt ma) => ma);

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act
        StartAttemptResponseDto result = await _useCase.Execute(request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Questions);
        Assert.Equal(question1.Id, result.Questions[0].Id);
        Assert.Equal("Q1", result.Questions[0].Statement);
        Assert.Equal(2, result.Questions[0].Options.Count);
        var optionLabels = result.Questions[0].Options.Select(o => o.Label).ToList();
        Assert.Contains("A", optionLabels);
        Assert.Contains("B", optionLabels);
    }

    [Fact]
    public async Task Execute_QuestionsAmount_LimitsQuestions()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var userGuid = Guid.Parse(userId);
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(userGuid)).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Active, AttemptsAmount = 2, QuizId = Guid.NewGuid(), QuestionsAmount = 1, ShuffleOptions = false, ShuffleQuestion = false };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);
        _mockMatchAttemptRepository.Setup(r => r.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, userGuid)).ReturnsAsync(0);
        _mockMatchAttemptRepository.Setup(r => r.HasActiveAttemptByMatchIdAsync(match.Id, userGuid)).ReturnsAsync(false);

        var quiz = new Quiz { Id = match.QuizId };
        _mockQuizRepository.Setup(q => q.GetByIdAsync(match.QuizId)).ReturnsAsync(quiz);

        var questions = new List<AugmentedQuestionDto> {
            new AugmentedQuestionDto(new Question { Id = Guid.NewGuid(), Content = "Q1", Options = new List<Option> { new Option { Id = Guid.NewGuid(), Description = "A" } } }, 1m),
            new AugmentedQuestionDto(new Question { Id = Guid.NewGuid(), Content = "Q2", Options = new List<Option> { new Option { Id = Guid.NewGuid(), Description = "B" } } }, 1m),
            new AugmentedQuestionDto(new Question { Id = Guid.NewGuid(), Content = "Q3", Options = new List<Option> { new Option { Id = Guid.NewGuid(), Description = "C" } } }, 1m)
        };
        _mockQuizQuestionRepository.Setup(q => q.GetQuestionsAndScoreByQuizIdAsync(quiz.Id)).ReturnsAsync(questions);

        _mockMatchAttemptRepository.Setup(r => r.AddMatchAttemptAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync((MatchAttempt ma) => ma);

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act
        StartAttemptResponseDto result = await _useCase.Execute(request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Questions);
    }

    [Fact]
    public async Task Execute_ShuffleQuestions_DeterministicWithSeed()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var userGuid = Guid.Parse(userId);
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(userGuid)).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Active, AttemptsAmount = 2, QuizId = Guid.NewGuid(), ShuffleOptions = false, ShuffleQuestion = true };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);
        _mockMatchAttemptRepository.Setup(r => r.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, userGuid)).ReturnsAsync(0);
        _mockMatchAttemptRepository.Setup(r => r.HasActiveAttemptByMatchIdAsync(match.Id, userGuid)).ReturnsAsync(false);

        var quiz = new Quiz { Id = match.QuizId };
        _mockQuizRepository.Setup(q => q.GetByIdAsync(match.QuizId)).ReturnsAsync(quiz);

        var q1Id = Guid.NewGuid();
        var q2Id = Guid.NewGuid();
        var q3Id = Guid.NewGuid();

        _mockQuizQuestionRepository
            .Setup(q => q.GetQuestionsAndScoreByQuizIdAsync(quiz.Id))
            .ReturnsAsync(() => new List<AugmentedQuestionDto>
            {
                new AugmentedQuestionDto(new Question { Id = q1Id, Content = "Q1", Options = new List<Option> { new Option { Id = Guid.NewGuid(), Description = "A" } } }, 1m),
                new AugmentedQuestionDto(new Question { Id = q2Id, Content = "Q2", Options = new List<Option> { new Option { Id = Guid.NewGuid(), Description = "B" } } }, 1m),
                new AugmentedQuestionDto(new Question { Id = q3Id, Content = "Q3", Options = new List<Option> { new Option { Id = Guid.NewGuid(), Description = "C" } } }, 1m)
            });

        _mockMatchAttemptRepository.Setup(r => r.AddMatchAttemptAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync((MatchAttempt ma) => ma);

        var seededRandom1 = new Random(42);
        var seededRandom2 = new Random(42);

        var useCase1 = new StartAttemptUseCase(
            _mockCurrentUser.Object,
            _mockCourseImpl.Object,
            _mockMatchRepository.Object,
            _mockQuizRepository.Object,
            _mockQuizQuestionRepository.Object,
            _mockMatchAttemptRepository.Object,
            seededRandom1
        );

        var useCase2 = new StartAttemptUseCase(
            _mockCurrentUser.Object,
            _mockCourseImpl.Object,
            _mockMatchRepository.Object,
            _mockQuizRepository.Object,
            _mockQuizQuestionRepository.Object,
            _mockMatchAttemptRepository.Object,
            seededRandom2
        );

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act
        var result1 = await useCase1.Execute(request);
        var result2 = await useCase2.Execute(request);

        // Assert
        Assert.Equal(result1.Questions.Select(q => q.Id).ToList(), result2.Questions.Select(q => q.Id).ToList());
    }

    [Fact]
    public async Task Execute_ShuffleOptions_DeterministicWithSeed()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var userGuid = Guid.Parse(userId);
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(userGuid)).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Active, AttemptsAmount = 2, QuizId = Guid.NewGuid(), ShuffleOptions = true, ShuffleQuestion = false };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);
        _mockMatchAttemptRepository.Setup(r => r.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, userGuid)).ReturnsAsync(0);
        _mockMatchAttemptRepository.Setup(r => r.HasActiveAttemptByMatchIdAsync(match.Id, userGuid)).ReturnsAsync(false);

        var quiz = new Quiz { Id = match.QuizId };
        _mockQuizRepository.Setup(q => q.GetByIdAsync(match.QuizId)).ReturnsAsync(quiz);

        var q1Id = Guid.NewGuid();
        var optAId = Guid.NewGuid();
        var optBId = Guid.NewGuid();
        var optCId = Guid.NewGuid();

        _mockQuizQuestionRepository
            .Setup(q => q.GetQuestionsAndScoreByQuizIdAsync(quiz.Id))
            .ReturnsAsync(() => new List<AugmentedQuestionDto>
            {
                new AugmentedQuestionDto(new Question { Id = q1Id, Content = "Q1", Options = new List<Option> {
                    new Option { Id = optAId, Description = "A" },
                    new Option { Id = optBId, Description = "B" },
                    new Option { Id = optCId, Description = "C" }
                }}, 1m)
            });

        _mockMatchAttemptRepository.Setup(r => r.AddMatchAttemptAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync((MatchAttempt ma) => ma);

        var seededRandom1 = new Random(42);
        var seededRandom2 = new Random(42);

        var useCase1 = new StartAttemptUseCase(
            _mockCurrentUser.Object,
            _mockCourseImpl.Object,
            _mockMatchRepository.Object,
            _mockQuizRepository.Object,
            _mockQuizQuestionRepository.Object,
            _mockMatchAttemptRepository.Object,
            seededRandom1
        );

        var useCase2 = new StartAttemptUseCase(
            _mockCurrentUser.Object,
            _mockCourseImpl.Object,
            _mockMatchRepository.Object,
            _mockQuizRepository.Object,
            _mockQuizQuestionRepository.Object,
            _mockMatchAttemptRepository.Object,
            seededRandom2
        );

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act
        var result1 = await useCase1.Execute(request);
        var result2 = await useCase2.Execute(request);

        // Assert
        Assert.Equal(result1.Questions.Select(q => q.Id).ToList(), result2.Questions.Select(q => q.Id).ToList());
        var opt1 = result1.Questions[0].Options.Select(o => o.Label).ToList();
        var opt2 = result2.Questions[0].Options.Select(o => o.Label).ToList();
        Assert.Equal(opt1, opt2);
    }

    [Fact]
    public async Task Execute_ShuffleQuestionsAndOptions_DeterministicWithSeed()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        var userGuid = Guid.Parse(userId);
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        var course = new CourseSummaryDTO { Id = Guid.NewGuid() };
        _mockCourseImpl.Setup(c => c.GetCoursesByStudent(userGuid)).ReturnsAsync(new List<CourseSummaryDTO> { course });

        var match = new Domain.Entities.Match { Id = Guid.NewGuid(), CourseId = course.Id, Status = MatchStatus.Active, AttemptsAmount = 2, QuizId = Guid.NewGuid(), ShuffleOptions = true, ShuffleQuestion = true };
        _mockMatchRepository.Setup(m => m.GetMatchByIdAsync(It.IsAny<Guid>())).ReturnsAsync(match);
        _mockMatchAttemptRepository.Setup(r => r.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, userGuid)).ReturnsAsync(0);
        _mockMatchAttemptRepository.Setup(r => r.HasActiveAttemptByMatchIdAsync(match.Id, userGuid)).ReturnsAsync(false);

        var quiz = new Quiz { Id = match.QuizId };
        _mockQuizRepository.Setup(q => q.GetByIdAsync(match.QuizId)).ReturnsAsync(quiz);

        var q1Id = Guid.NewGuid();
        var q2Id = Guid.NewGuid();
        var optAId = Guid.NewGuid(); var optBId = Guid.NewGuid(); var optCId = Guid.NewGuid();
        var optDId = Guid.NewGuid(); var optEId = Guid.NewGuid(); var optFId = Guid.NewGuid();

        _mockQuizQuestionRepository
            .Setup(q => q.GetQuestionsAndScoreByQuizIdAsync(quiz.Id))
            .ReturnsAsync(() => new List<AugmentedQuestionDto>
            {
                new AugmentedQuestionDto(new Question { Id = q1Id, Content = "Q1", Options = new List<Option> {
                    new Option { Id = optAId, Description = "A" },
                    new Option { Id = optBId, Description = "B" },
                    new Option { Id = optCId, Description = "C" }
                }}, 1m),
                new AugmentedQuestionDto(new Question { Id = q2Id, Content = "Q2", Options = new List<Option> {
                    new Option { Id = optDId, Description = "D" },
                    new Option { Id = optEId, Description = "E" },
                    new Option { Id = optFId, Description = "F" }
                }}, 1m)
            });

        _mockMatchAttemptRepository.Setup(r => r.AddMatchAttemptAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync((MatchAttempt ma) => ma);

        var seededRandom1 = new Random(42);
        var seededRandom2 = new Random(42);

        var useCase1 = new StartAttemptUseCase(
            _mockCurrentUser.Object,
            _mockCourseImpl.Object,
            _mockMatchRepository.Object,
            _mockQuizRepository.Object,
            _mockQuizQuestionRepository.Object,
            _mockMatchAttemptRepository.Object,
            seededRandom1
        );

        var useCase2 = new StartAttemptUseCase(
            _mockCurrentUser.Object,
            _mockCourseImpl.Object,
            _mockMatchRepository.Object,
            _mockQuizRepository.Object,
            _mockQuizQuestionRepository.Object,
            _mockMatchAttemptRepository.Object,
            seededRandom2
        );

        var request = new StartAttemptRequestDto { MatchId = match.Id };

        // Act
        var result1 = await useCase1.Execute(request);
        var result2 = await useCase2.Execute(request);

        // Assert
        Assert.Equal(result1.Questions.Select(q => q.Id).ToList(), result2.Questions.Select(q => q.Id).ToList());
        for (int i = 0; i < result1.Questions.Count; i++)
        {
            var o1 = result1.Questions[i].Options.Select(o => o.Label).ToList();
            var o2 = result2.Questions[i].Options.Select(o => o.Label).ToList();
            Assert.Equal(o1, o2);
        }
    }
}
