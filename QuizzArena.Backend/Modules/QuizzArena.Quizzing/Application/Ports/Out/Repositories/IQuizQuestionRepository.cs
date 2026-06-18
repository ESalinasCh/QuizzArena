using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IQuizQuestionRepository
{
    public Task CreateMultipleAsync(IEnumerable<QuizQuestion> questions);

    Task<List<Question>> GetQuestionsByQuizIdAsync(Guid QuizId);
}
