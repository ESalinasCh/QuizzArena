using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IQuizQuestionRepository
{
    Task<List<Question>> GetQuestionsByQuizIdAsync(Guid QuizId);
}
