
using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators.Quiz;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz;

internal class CreateQuizUseCase(
    IQuizRepository repository,
    IMapper mapper,
    CreateQuizDtoValidator createValidator,
    ICreateQuestionsUseCase createQuestionsUseCase,
    ICreateOptionsUseCase createOptionsUseCase
) : ICreateQuizUseCase
{
    public async Task<QuizDto> Execute(CreateQuizDto dto, Guid classSourceId)
    {
        await createValidator.ValidateAndThrowAsync(dto);
        Domain.Entities.Quiz quiz = mapper.Map<Domain.Entities.Quiz>(dto);
        await repository.CreateAsync(quiz);

        List<CreateQuestionDto> questions = [];
        List<CreateOptionDto> options = [];

        foreach (CreateQuestionDto question in dto.QuizQuestions)
        {
            foreach (CreateOptionDto option in question.Options)
            {
                option.QuestionId = question.Id;
                options.Add(option);
            }
            questions.Add(question);
        }
        await createQuestionsUseCase.Execute(questions, quiz.Id);
        await createOptionsUseCase.Execute(options);
        return mapper.Map<QuizDto>(quiz);
    }
}
