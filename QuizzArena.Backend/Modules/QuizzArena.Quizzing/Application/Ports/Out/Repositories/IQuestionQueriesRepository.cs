
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IQuestionQueriesRepository
{
    Task<List<Question>> GetQuestionsByIds(List<Guid> ids);
}
