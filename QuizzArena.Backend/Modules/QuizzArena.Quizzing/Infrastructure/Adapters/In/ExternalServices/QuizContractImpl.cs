using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.ExternalServices;

public class QuizContract(
    IQuizRepository quizRepository
) : IQuizContract
{

    public async Task<Guid> CreateQuiz(QuizCreationRequestDTO quizRequestDto)
    {
        Quiz quiz = new Quiz()
        {
            Id = quizRequestDto.Id,
            Title = quizRequestDto.Title,
            Description = quizRequestDto.Description,
            Status = QuizStatus.draft,
            Origin = QuizOrigin.AiGenerated,
            Deleted = false,
            QuizQuestions = quizRequestDto.Questions.Select(q => new QuizQuestion()
            {
                Id = Guid.NewGuid(),
                Position = q.Position,
                ValueScore = q.ValueScore,
                Deleted = false,
                QuizId = quizRequestDto.Id,
                QuestionId = q.QuestionId,
            }).ToList()
        };

        await quizRepository.CreateAsync(quiz);

        return quizRequestDto.Id;
    }
}
