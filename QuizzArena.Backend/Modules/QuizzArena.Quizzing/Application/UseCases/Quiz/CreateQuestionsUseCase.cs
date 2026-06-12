using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz;

internal class CreateQuestionsUseCase(
    IQuestionRepository repository,
    IMapper mapper,
    CreateQuestionsDtoValidator createValidator
) : ICreateQuestionsUseCase
{
    public async Task Execute(IEnumerable<CreateQuestionDto> dtos, Guid classSourceId)
    {
        await createValidator.ValidateAndThrowAsync(dtos);

        IEnumerable<Question> questions = dtos.Select(dto =>
        {
            Question question = mapper.Map<Question>(dto);
            question.ProcessingJobId = classSourceId;
            return question;
        });

        await repository.CreateMultipleAsync(questions);
    }
}
