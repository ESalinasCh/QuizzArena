using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IQuizRepository
{
    public Task<Quiz?> GetByIdAsync(Guid classSourceId);
    public Task<Quiz> CreateAsync(Quiz quiz);

    Task<Quiz?> GetQuizByIdAsync(Guid quizId);
}
