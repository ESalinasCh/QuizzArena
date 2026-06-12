using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

internal interface IQuizQuestionRepository
{
    Task<List<Question>> GetQuestionsByQuizIdAsync(Guid QuizId);
}
