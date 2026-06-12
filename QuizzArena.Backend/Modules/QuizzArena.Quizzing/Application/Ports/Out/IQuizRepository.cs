using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

internal interface IQuizRepository
{
    Task<Quiz?> GetQuizByIdAsync(Guid quizId);
}
