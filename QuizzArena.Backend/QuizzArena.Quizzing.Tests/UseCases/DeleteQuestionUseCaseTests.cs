using AutoMapper;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.UseCases.QuestionUseCases;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class DeleteQuestionUseCaseTests
{
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly DeleteQuestionUseCase _useCase;

    public DeleteQuestionUseCaseTests()
    {
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockMapper = new Mock<IMapper>();

        _mockMapper
            .Setup(m => m.Map<ResponseQuestionDto>(It.IsAny<Question>()))
            .Returns(new ResponseQuestionDto());

        _useCase = new DeleteQuestionUseCase(
            _mockQuestionRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Execute_QuestionDoesNotExist_ThrowsInvalidOperationException()
    {
        Guid questionId = Guid.NewGuid();

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync((Question?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(questionId));

        _mockQuestionRepository.Verify(x => x.UpdateAsync(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ExistingQuestion_SoftDeletesQuestionAndOptionsAndCallsUpdateAsyncOnce()
    {
        Guid questionId = Guid.NewGuid();
        Question question = new()
        {
            Id = questionId,
            Options =
            [
                new Option { Id = Guid.NewGuid(), Position = 1 },
                new Option { Id = Guid.NewGuid(), Position = 2 }
            ]
        };

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(question);

        await _useCase.Execute(questionId);

        Assert.True(question.Deleted);
        Assert.NotNull(question.DeletedAt);
        Assert.All(question.Options, o =>
        {
            Assert.True(o.Deleted);
            Assert.NotNull(o.DeletedAt);
        });

        _mockQuestionRepository.Verify(
            x => x.UpdateAsync(It.Is<Question>(q => q.Id == questionId && q.Deleted)),
            Times.Once);
    }
}
