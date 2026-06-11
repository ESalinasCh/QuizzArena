using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz;

internal class CreateQuestionsUseCase(
    IQuestionRepository repository,
    IMapper mapper,
    CreateQuestionsDtoValidator createValidator,
    CreateOptionsUseCase createOptionsUseCase
) : ICreateQuestionsUseCase
{
    public async Task Execute(IEnumerable<CreateQuestionDto> dtos, Guid classSourceId)
    {
        await createValidator.ValidateAndThrowAsync(dtos);
        List<Question> questions = [];
        List<CreateOptionDto> options = [];

        foreach (CreateQuestionDto dto in dtos)
        {
            foreach (CreateOptionDto option in dto.Options)
            {
                option.QuestionId = dto.Id;
                options.Add(option);
            }

            Question question = mapper.Map<Question>(dto);

            question.ProcessingJobId = classSourceId;
            questions.Add(question);
        }

        await repository.CreateMultipleAsync(questions);

        await createOptionsUseCase.Execute(options);
    }
}
