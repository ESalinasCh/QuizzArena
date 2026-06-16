using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IQuizRepository
{
    Task<Quiz?> GetQuizByIdAsync(Guid quizId);
}
