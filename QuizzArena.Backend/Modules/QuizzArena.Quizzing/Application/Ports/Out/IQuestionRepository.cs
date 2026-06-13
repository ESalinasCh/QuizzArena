
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IQuestionRepository
{
    Task<List<Question>> GetByIdsAsync(List<Guid> questionIds);
    Task<List<Question>> GetByIdsWithOptionsAsync(List<Guid> questionIds);
}
