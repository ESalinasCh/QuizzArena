using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IQuizAttemptRepository
{
    Task<QuizAttempt> Start();
    Task<QuizAttempt> End();
}
