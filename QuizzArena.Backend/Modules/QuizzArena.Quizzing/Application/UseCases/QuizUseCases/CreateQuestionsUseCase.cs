using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.QuizUseCases;

public class CreateQuestionsUseCase(
    IQuestionRepository questionRepository,
    IQuizQuestionRepository quizQuestionRepository,
    IMapper mapper,
    CreateQuestionsDtoValidator createValidator
) : ICreateQuestionsUseCase
{
    public async Task Execute(IEnumerable<CreateQuestionDto> dtos, Guid classSourceId, Guid quizId)
    {
        await createValidator.ValidateAndThrowAsync(dtos);
        List<Question> questions = [];
        List<QuizQuestion> quizQuestions = [];

        foreach (CreateQuestionDto dto in dtos)
        {
            Question question = mapper.Map<Question>(dto);
            question.ProcessingJobId = classSourceId;
            questions.Add(question);
            quizQuestions.Add(new QuizQuestion
            {
                QuizId = quizId,
                QuestionId = question.Id,
                Position = dto.Position,
                ValueScore = dto.ValueScore
            });
        }

        await questionRepository.CreateMultipleAsync(questions);
        await quizQuestionRepository.CreateMultipleAsync(quizQuestions);
    }
}
