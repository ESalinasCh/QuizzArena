using AutoMapper;
using FluentValidation;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.UseCases.QuestionUseCases;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class UpdateQuestionUseCaseTests
{
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UpdateQuestionUseCase _useCase;

    public UpdateQuestionUseCaseTests()
    {
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockMapper = new Mock<IMapper>();

        _mockMapper
            .Setup(m => m.Map<ResponseQuestionDto>(It.IsAny<Question>()))
            .Returns(new ResponseQuestionDto());

        _useCase = new UpdateQuestionUseCase(
            _mockQuestionRepository.Object,
            _mockMapper.Object,
            new UpdateQuestionDtoValidator());
    }

    [Fact]
    public async Task Execute_QuestionDoesNotExist_ThrowsInvalidOperationException()
    {
        Guid questionId = Guid.NewGuid();

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync((Question?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(new UpdateQuestionDto { QuestionId = questionId, Content = "New" }));
    }

    [Fact]
    public async Task Execute_MissingQuestionId_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _useCase.Execute(new UpdateQuestionDto { QuestionId = Guid.Empty }));

        _mockQuestionRepository.Verify(x => x.UpdateAsync(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ProvidedContentIsEmpty_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _useCase.Execute(new UpdateQuestionDto { QuestionId = Guid.NewGuid(), Content = string.Empty }));

        _mockQuestionRepository.Verify(x => x.UpdateAsync(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task Execute_OnlyProvidedFieldsAreUpdated()
    {
        Guid questionId = Guid.NewGuid();
        Question question = new()
        {
            Id = questionId,
            Content = "Original content",
            Justification = "Original justification",
            Status = QuestionStatus.Draft,
            Type = QuestionType.MultipleChoice
        };

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(question);

        // Only justification is sent; everything else must stay untouched.
        await _useCase.Execute(new UpdateQuestionDto
        {
            QuestionId = questionId,
            Justification = "Updated justification"
        });

        Assert.Equal("Original content", question.Content);
        Assert.Equal("Updated justification", question.Justification);
        Assert.Equal(QuestionStatus.Draft, question.Status);
        Assert.Equal(QuestionType.MultipleChoice, question.Type);

        _mockQuestionRepository.Verify(x => x.UpdateAsync(It.Is<Question>(q => q.Id == questionId)), Times.Once);
    }

    [Fact]
    public async Task Execute_OptionUpdate_UpdatesOnlyProvidedSubfieldsAndLeavesOthersUntouched()
    {
        Guid questionId = Guid.NewGuid();
        Guid targetOptionId = Guid.NewGuid();
        Guid otherOptionId = Guid.NewGuid();

        Option target = new() { Id = targetOptionId, Description = "Old", IsCorrect = false, Position = 1 };
        Option other = new() { Id = otherOptionId, Description = "Untouched", IsCorrect = true, Position = 2 };

        Question question = new() { Id = questionId, Options = [target, other] };

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(question);

        await _useCase.Execute(new UpdateQuestionDto
        {
            QuestionId = questionId,
            Options =
            [
                new UpdateOptionDto { OptionId = targetOptionId, Description = "New description" }
            ]
        });

        // Only the provided sub-field changed on the target option.
        Assert.Equal("New description", target.Description);
        Assert.False(target.IsCorrect);
        Assert.Equal(1, target.Position);

        // The option not present in the payload is completely untouched (not deleted).
        Assert.Equal("Untouched", other.Description);
        Assert.False(other.Deleted);

        _mockQuestionRepository.Verify(x => x.UpdateAsync(It.IsAny<Question>()), Times.Once);
    }

    [Fact]
    public async Task Execute_OptionIdNotFound_ThrowsInvalidOperationException()
    {
        Guid questionId = Guid.NewGuid();
        Question question = new()
        {
            Id = questionId,
            Options = [new Option { Id = Guid.NewGuid(), Position = 1 }]
        };

        _mockQuestionRepository
            .Setup(x => x.GetByIdWithOptionsAsync(questionId))
            .ReturnsAsync(question);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.Execute(new UpdateQuestionDto
            {
                QuestionId = questionId,
                Options = [new UpdateOptionDto { OptionId = Guid.NewGuid(), Description = "x" }]
            }));
    }
}
