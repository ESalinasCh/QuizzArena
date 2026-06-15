using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators.Option;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz;

internal class CreateOptionsUseCase(
    IOptionRepository repository,
    IMapper mapper,
    CreateOptionsDtoValidator createValidator
) : ICreateOptionsUseCase
{
    public async Task Execute(IEnumerable<CreateOptionDto> dtos)
    {
        await createValidator.ValidateAndThrowAsync(dtos);
        List<Option> options = [..dtos.Select(dto =>
        {
            Option option = mapper.Map<Option>(dto);
            return option;
        })];

        await repository.CreateMultipleAsync(options);
    }
}
