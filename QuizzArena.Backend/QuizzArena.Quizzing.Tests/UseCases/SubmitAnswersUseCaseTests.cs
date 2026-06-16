
using AutoMapper;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;
using QuizzArena.Quizzing.Application.Validators;
using QuizzArena.Quizzing.Domain.Entities;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class SubmitAnswersUseCaseTests
{
    // Mocks
    private readonly Mock<IOptionRepository> _mockOptionRepository;
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
        _mockOptionRepository = new Mock<IOptionRepository>();
        _mockMatchAttemptRepository = new Mock<IMatchAttemptRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _submitAnswersValidator = new SubmitAnswersRequestValidator();

        _submitAnswersUseCase = new SubmitAnswersUseCase(
        _submitAnswersValidator,
            _mockOptionRepository.Object,
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
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();

        _mockCurrentUser.Setup(user => user.UserId).Returns(userId);

        var matchAttempt = new MatchAttempt { Id = matchAttemptId, UserId = Guid.Parse(userId) };
        _mockMatchAttemptRepository
            .Setup(repo => repo.GetByIdAsync(matchAttemptId))
            .ReturnsAsync(matchAttempt);

        var dto = new SubmitAnswersRequestDto
        {
            Answers = [new SubmitAnswerBody(questionId, optionId, DateTimeOffset.UtcNow.AddMinutes(-1))]
        };

        var answers = new List<Answer> { new Answer { QuestionId = questionId, OptionId = optionId } };
        _mockMapper.Setup(m => m.Map<List<Answer>>(dto.Answers)).Returns(answers);

        var option = new Option { Id = optionId, IsCorrect = true };
        _mockOptionRepository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([option]);

        _mockMatchAttemptRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<MatchAttempt>()))
            .ReturnsAsync(matchAttempt);

        var question = new Question { Id = questionId, Content = "Sample question", Options = [option] };
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
}
