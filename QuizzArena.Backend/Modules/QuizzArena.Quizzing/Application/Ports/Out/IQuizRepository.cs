using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IQuizRepository
{
    Task<Quiz> GetById(Guid id);
    Task<Quiz> Create(Guid id);
    Task<Quiz> Update(Guid id);
    Task Delete(Guid id);
}
