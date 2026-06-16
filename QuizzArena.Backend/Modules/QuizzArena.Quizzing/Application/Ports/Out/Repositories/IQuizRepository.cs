using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IQuizRepository
{
    Task<Quiz?> GetQuizByIdAsync(Guid quizId);
}
