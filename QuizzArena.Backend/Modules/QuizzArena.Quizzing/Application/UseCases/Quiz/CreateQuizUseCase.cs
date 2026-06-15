using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators.Quiz;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz;

public class CreateQuizUseCase(
    IQuizRepository repository,
    IMapper mapper,
    CreateQuizDtoValidator createValidator,
    ICreateQuestionsUseCase createQuestionsUseCase,
    ICreateOptionsUseCase createOptionsUseCase
) : ICreateQuizUseCase
{
    public async Task Execute(CreateQuizDto dto, Guid classSourceId)
    {
        await createValidator.ValidateAndThrowAsync(dto);
        Domain.Entities.Quiz quiz = mapper.Map<Domain.Entities.Quiz>(dto);
        Domain.Entities.Quiz newQuiz = await repository.CreateAsync(quiz);

        foreach (CreateQuestionDto question in dto.Questions)
        {
            question.Options.ForEach(option =>
                option.QuestionId = question.Id);
        }

        await createQuestionsUseCase.Execute(dto.Questions, classSourceId, newQuiz.Id);
        await createOptionsUseCase.Execute(dto.Questions.SelectMany(q => q.Options));
    }
}
