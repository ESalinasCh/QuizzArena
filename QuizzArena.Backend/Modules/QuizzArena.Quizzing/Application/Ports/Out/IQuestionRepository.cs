using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IQuestionRepository
{
    public Task CreateMultipleAsync(IEnumerable<Question> questions);
    Task<List<Question>> GetByIdsAsync(IEnumerable<Guid> questionIds);
    Task<List<Question>> GetActiveByIdsAsync(IEnumerable<Guid> questionIds);
    Task<List<Question>> GetByIdsWithOptionsAsync(List<Guid> questionIds);
    Task<List<Question>> GetByProcessingJobIdAsync(QuestionFilters filters);
    Task<Question?> GetByIdWithOptionsAsync(Guid questionId);
    Task<Question> UpdateAsync(Question question);
}
