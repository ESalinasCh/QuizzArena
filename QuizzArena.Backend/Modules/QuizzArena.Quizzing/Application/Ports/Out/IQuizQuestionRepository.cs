using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IQuizQuestionRepository
{
    Task<List<Question>> GetQuestionsByQuizIdAsync(Guid QuizId);
}
