using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Moq;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.QuestionUseCases;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using static MassTransit.ValidationResultExtensions;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class GetQuestionsUseCaseTests
{
    // Mocks
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IMapper> _mockMapper;

    // Target unit test
    private readonly GetQuestionsUseCase _getQuestionsUseCase;

    public GetQuestionsUseCaseTests()
    {
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockMapper = new Mock<IMapper>();

        _getQuestionsUseCase = new GetQuestionsUseCase(
            _mockQuestionRepository.Object,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task Execute_ValidOptions_CallsRepositoryCreateMultipleAsync()
    {
        // Arrange
        Guid processingJobId = Guid.NewGuid();
        QuestionFilters filters = new QuestionFilters()
        {
            Page = 1,
            PageSize = 10,
            ProcessingJobIds = [processingJobId],
            Status = QuestionStatus.Draft
        };

        List<Question> questions =
        [
            new Question
            {
                Id = Guid.NewGuid(),
                Content = "What is 2 + 2?",
                Type = QuestionType.MultipleChoice,
                Justification = "The sum of 2 and 2 is 4.",
                ProcessingJobId = processingJobId,
                Status = QuestionStatus.Draft,
                Deleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new Question
            {
                Id = Guid.NewGuid(),
                Content = "What is the capital of France?",
                Type = QuestionType.MultipleChoice,
                Justification = "The capital of France is Paris.",
                ProcessingJobId = processingJobId,
                Status = QuestionStatus.Draft,
                Deleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        ];

        List<ResponseQuestionDto> responseQuestionDto =
        [
            new ResponseQuestionDto
            {
                Id = Guid.NewGuid(),
                Content = "What is 2 + 2?",
                Type = QuestionType.MultipleChoice,
                Justification = "The sum of 2 and 2 is 4.",
                ProcessingJobId = processingJobId,
                Status = QuestionStatus.Draft,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new ResponseQuestionDto
            {
                Id = Guid.NewGuid(),
                Content = "What is the capital of France?",
                Type = QuestionType.MultipleChoice,
                Justification = "The capital of France is Paris.",
                ProcessingJobId = processingJobId,
                Status = QuestionStatus.Draft,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        ];

        _mockQuestionRepository
            .Setup(r => r.GetByProcessingJobIdAsync(filters))
            .ReturnsAsync(questions);

        _mockMapper.Setup(m => m.Map<ResponseQuestionDto>(It.IsAny<Question>()))
            .Returns((Question dto) => responseQuestionDto.First(q => q.Content == dto.Content));

        // Act
        List<ResponseQuestionDto> result = await _getQuestionsUseCase.Execute(filters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(questions[0].Content, result[0].Content);

        Assert.Equal(questions[1].Content, result[1].Content);

        _mockQuestionRepository.Verify(r => r.GetByProcessingJobIdAsync(filters),Times.Once);

        _mockMapper.Verify(m => m.Map<ResponseQuestionDto>(It.IsAny<Question>()),Times.Exactly(questions.Count));
    }
}
