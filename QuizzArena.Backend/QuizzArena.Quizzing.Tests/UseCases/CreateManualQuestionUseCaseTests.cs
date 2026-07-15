using AutoMapper;
using FluentValidation;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.UseCases.QuestionUseCases;
using QuizzArena.Quizzing.Application.Validators.Option;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class CreateManualQuestionUseCaseTests
{
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IOptionRepository> _mockOptionRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreateManualQuestionUseCase _useCase;

    public CreateManualQuestionUseCaseTests()
    {
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockOptionRepository = new Mock<IOptionRepository>();
        _mockMapper = new Mock<IMapper>();

        _mockMapper
            .Setup(m => m.Map<Question>(It.IsAny<CreateManualQuestionDto>()))
            .Returns((CreateManualQuestionDto d) => new Question { Content = d.Content });

        _mockMapper
            .Setup(m => m.Map<Option>(It.IsAny<CreateOptionDto>()))
            .Returns((CreateOptionDto o) => new Option
            {
                Description = o.Description,
                IsCorrect = o.IsCorrect,
                Position = o.Position
            });

        _mockMapper
            .Setup(m => m.Map<ResponseQuestionDto>(It.IsAny<Question>()))
            .Returns((Question q) => new ResponseQuestionDto { Id = q.Id });

        _useCase = new CreateManualQuestionUseCase(
            _mockQuestionRepository.Object,
            _mockOptionRepository.Object,
            _mockMapper.Object,
            new CreateManualQuestionDtoValidator(new CreateOptionDtoValidator()));
    }

    private static CreateManualQuestionDto ValidDto() => new()
    {
        Content = "What is 2 + 2?",
        Justification = "Basic arithmetic",
        Type = QuestionType.SingleChoice,
        ProcessingJobId = Guid.NewGuid(),
        Options =
        [
            new CreateOptionDto { Description = "3", IsCorrect = false, Position = 1 },
            new CreateOptionDto { Description = "4", IsCorrect = true, Position = 2 }
        ]
    };

    [Fact]
    public async Task Execute_ValidQuestion_PersistsQuestionAndOptions()
    {
        List<Question>? savedQuestions = null;
        List<Option>? savedOptions = null;

        _mockQuestionRepository
            .Setup(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()))
            .Callback((IEnumerable<Question> q) => savedQuestions = q.ToList())
            .Returns(Task.CompletedTask);

        _mockOptionRepository
            .Setup(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Option>>()))
            .Callback((IEnumerable<Option> o) => savedOptions = o.ToList())
            .Returns(Task.CompletedTask);

        ResponseQuestionDto response = await _useCase.Execute(ValidDto());

        Assert.NotNull(savedQuestions);
        Question savedQuestion = Assert.Single(savedQuestions!);
        Assert.NotEqual(Guid.Empty, savedQuestion.Id);
        Assert.Equal(QuestionOrigin.ManuallyCreated, savedQuestion.Origin);

        Assert.NotNull(savedOptions);
        Assert.Equal(2, savedOptions!.Count);
        Assert.All(savedOptions!, o => Assert.Equal(savedQuestion.Id, o.QuestionId));

        Assert.Equal(savedQuestion.Id, response.Id);

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Once);
        _mockOptionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Option>>()), Times.Once);
    }

    [Fact]
    public async Task Execute_EmptyContent_ThrowsValidationException()
    {
        CreateManualQuestionDto dto = ValidDto();
        dto.Content = string.Empty;

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
        _mockOptionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Option>>()), Times.Never);
    }

    [Fact]
    public async Task Execute_EmptyProcessingJobId_ThrowsValidationException()
    {
        CreateManualQuestionDto dto = ValidDto();
        dto.ProcessingJobId = Guid.Empty;

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
    }

    [Fact]
    public async Task Execute_NoOptions_ThrowsValidationException()
    {
        CreateManualQuestionDto dto = ValidDto();
        dto.Options = [];

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
    }

    [Fact]
    public async Task Execute_MoreThanFourOptions_ThrowsValidationException()
    {
        CreateManualQuestionDto dto = ValidDto();
        dto.Options =
        [
            new CreateOptionDto { Description = "a", IsCorrect = true, Position = 1 },
            new CreateOptionDto { Description = "b", IsCorrect = false, Position = 2 },
            new CreateOptionDto { Description = "c", IsCorrect = false, Position = 3 },
            new CreateOptionDto { Description = "d", IsCorrect = false, Position = 4 },
            new CreateOptionDto { Description = "e", IsCorrect = false, Position = 5 }
        ];

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
        _mockOptionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Option>>()), Times.Never);
    }

    [Fact]
    public async Task Execute_NonConsecutivePositions_ThrowsValidationException()
    {
        CreateManualQuestionDto dto = ValidDto();
        dto.Options =
        [
            new CreateOptionDto { Description = "a", IsCorrect = true, Position = 1 },
            new CreateOptionDto { Description = "b", IsCorrect = false, Position = 2 },
            new CreateOptionDto { Description = "c", IsCorrect = false, Position = 3 },
            new CreateOptionDto { Description = "d", IsCorrect = false, Position = 9 }
        ];

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
        _mockOptionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Option>>()), Times.Never);
    }

    [Fact]
    public async Task Execute_FourConsecutiveOptions_Persists()
    {
        CreateManualQuestionDto dto = ValidDto();
        dto.Options =
        [
            new CreateOptionDto { Description = "a", IsCorrect = true, Position = 1 },
            new CreateOptionDto { Description = "b", IsCorrect = false, Position = 2 },
            new CreateOptionDto { Description = "c", IsCorrect = false, Position = 3 },
            new CreateOptionDto { Description = "d", IsCorrect = false, Position = 4 }
        ];

        await _useCase.Execute(dto);

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Once);
        _mockOptionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Option>>()), Times.Once);
    }

    [Fact]
    public async Task Execute_SingleChoiceWithMultipleCorrect_ThrowsValidationException()
    {
        CreateManualQuestionDto dto = ValidDto();
        dto.Type = QuestionType.SingleChoice;
        dto.Options =
        [
            new CreateOptionDto { Description = "3", IsCorrect = false, Position = 1 },
            new CreateOptionDto { Description = "4", IsCorrect = true, Position = 2 },
            new CreateOptionDto { Description = "3", IsCorrect = false, Position = 3 },
            new CreateOptionDto { Description = "4", IsCorrect = true, Position = 4 }
        ];

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
        _mockOptionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Option>>()), Times.Never);
    }

    [Fact]
    public async Task Execute_MultipleChoiceWithMultipleCorrect_Persists()
    {
        CreateManualQuestionDto dto = ValidDto();
        dto.Type = QuestionType.MultipleChoice;
        dto.Options =
        [
            new CreateOptionDto { Description = "3", IsCorrect = false, Position = 1 },
            new CreateOptionDto { Description = "4", IsCorrect = true, Position = 2 },
            new CreateOptionDto { Description = "2+2", IsCorrect = true, Position = 3 }
        ];

        await _useCase.Execute(dto);

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Once);
        _mockOptionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Option>>()), Times.Once);
    }

    [Fact]
    public async Task Execute_NoCorrectOption_ThrowsValidationException()
    {
        CreateManualQuestionDto dto = ValidDto();
        dto.Options =
        [
            new CreateOptionDto { Description = "3", IsCorrect = false, Position = 1 },
            new CreateOptionDto { Description = "5", IsCorrect = false, Position = 2 }
        ];

        await Assert.ThrowsAsync<ValidationException>(() => _useCase.Execute(dto));

        _mockQuestionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Question>>()), Times.Never);
        _mockOptionRepository.Verify(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<Option>>()), Times.Never);
    }
}
