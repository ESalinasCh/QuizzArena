using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.UseCases.QuestionUseCases;

public class CreateManualQuestionUseCase(
    IQuestionRepository questionRepository,
    IOptionRepository optionRepository,
    IMapper mapper,
    CreateManualQuestionDtoValidator validator
) : ICreateManualQuestionUseCase
{
    public async Task<ResponseQuestionDto> Execute(CreateManualQuestionDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        Question question = mapper.Map<Question>(dto);
        question.Id = Guid.NewGuid();
        question.Origin = QuestionOrigin.ManuallyCreated;

        List<Option> options = dto.Options.Select(o =>
        {
            Option option = mapper.Map<Option>(o);
            option.QuestionId = question.Id;
            return option;
        }).ToList();

        await questionRepository.CreateMultipleAsync([question]);
        await optionRepository.CreateMultipleAsync(options);

        question.Options = options;

        return mapper.Map<ResponseQuestionDto>(question);
    }
}
