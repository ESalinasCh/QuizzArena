using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IAnswerRepository
{
    Task<Answer?> GetByAttemptAndQuestionAsync(Guid attemptId, Guid questionId);
    Task<Answer> CreateAnswerAsync(Answer answer);
    Task<Answer> UpdateAnswerAsync(Answer answer);

}
