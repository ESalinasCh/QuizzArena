using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Helpers;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.Validators.Quiz;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.UseCases.QuizUseCases;

public class CreateExamUseCase(
    IQuestionRepository questionRepository,
    IQuizRepository repository,
    IMapper mapper,
    CreateExamDtoValidator createValidator
    ) : ICreateExamUseCase
{
    public async Task<CreateQuizResponseDto> Execute(CreateExamDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);

        List<Question> questions = await questionRepository.GetByIdsAsync(dto.QuestionIds.ToList());

        if (questions.Count != dto.QuestionIds.Count())
        {
            throw new ValidationException("One or more question IDs do not exist.");
        }

        //TODO: Add score on the DTO so it can calculate Value Score with that too
        int valueScore = CalculateScoreValueHelper.Resolve(questions.Count, 100);

        Quiz quiz = mapper.Map<Quiz>(dto);

        quiz.Origin = QuizOrigin.ManuallyCreated;

        quiz.QuizQuestions = questions
            .Select((question, index) => new QuizQuestion
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                QuestionId = question.Id,
                Position = index + 1,
                ValueScore = valueScore
            })
            .ToList();

        Quiz createdQuiz = await repository.CreateAsync(quiz);

        CreateQuizResponseDto response = mapper.Map<CreateQuizResponseDto>(createdQuiz);

        return response;
    }
}
