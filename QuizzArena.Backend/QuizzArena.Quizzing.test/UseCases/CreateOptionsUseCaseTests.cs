using AutoMapper;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.UseCases.Quiz;
using QuizzArena.Quizzing.Application.Validators.Option;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.test.UseCases;

public class CreateOptionsUseCaseTests
{
    // Mocks
    private readonly Mock<IOptionRepository> _mockOptionRepository;
    private readonly Mock<IMapper> _mockMapper;

    // Real
    private readonly CreateOptionsDtoValidator _createOptionsDtoValidator;
    private readonly CreateOptionDtoValidator _createOptionDtoValidator;

    // Target unit test
    private readonly CreateOptionsUseCase _createOptionsUseCase;

    public CreateOptionsUseCaseTests()
    {
        _mockOptionRepository = new Mock<IOptionRepository>();
        _mockMapper = new Mock<IMapper>();

        _createOptionDtoValidator = new CreateOptionDtoValidator();
        _createOptionsDtoValidator = new CreateOptionsDtoValidator(_createOptionDtoValidator);

        _createOptionsUseCase = new CreateOptionsUseCase(
            _mockOptionRepository.Object,
            _mockMapper.Object,
            _createOptionsDtoValidator
        );
    }

    [Fact]
    public async Task Execute_ValidOptions_CallsRepositoryCreateMultipleAsync()
    {
        // Arrange
        List<CreateOptionDto> dtos =
        [
            new CreateOptionDto
            {
                Description = "3",
                Position = 1,
                IsCorrect = false
            },
            new CreateOptionDto
            {
                Description = "4",
                Position = 2,
                IsCorrect = true
            }
        ];

        List<Option> mappedOptions =
        [
            new Option
            {
                Id = Guid.NewGuid(),
                Description = "3",
                Position = 1,
                IsCorrect = false,
                Deleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Option
            {
                Id = Guid.NewGuid(),
                Description = "4",
                Position = 2,
                IsCorrect = true,
                Deleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ];

        _mockMapper.Setup(m => m.Map<Option>(It.IsAny<CreateOptionDto>()))
            .Returns((CreateOptionDto dto) => mappedOptions.First(o => o.Description == dto.Description));

        // Act
        await _createOptionsUseCase.Execute(dtos);

        // Assert
        _mockMapper.Verify(m => m.Map<Option>(It.IsAny<CreateOptionDto>()),Times.Exactly(dtos.Count));

        _mockOptionRepository.Verify(
            r => r.CreateMultipleAsync(It.Is<List<Option>>(options =>
                options.Count == 2 &&
                options.Any(o => o.Description == "3") &&
                options.Any(o => o.Description == "4"))),
            Times.Once);
    }
}
