using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IQuestionRepository
{
    Task<Question> GetById(Guid id);
    Task<Question> Create(Guid id);
    Task<Question> Update(Guid id);
    Task Delete(Guid id);
}
