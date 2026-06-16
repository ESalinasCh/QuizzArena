using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlQuizRepository(QuizzingDbContext context) : IQuizRepository
{
    public async Task<Quiz?> GetQuizByIdAsync(Guid quizId)
    {
        Quiz? quiz = await context.Quizzes.FindAsync(quizId);
        return quiz;
    }
}
