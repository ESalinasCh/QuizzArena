using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlQuizRepository(QuizzingDbContext context) : IQuizRepository
{
    public async Task<Quiz?> GetByIdAsync(Guid quizId)
    {
        return await context.Quizzes.FindAsync(quizId);
    }

    public async Task<Quiz> CreateAsync(Quiz quiz)
    {
        context.Quizzes.Add(quiz);
        await context.SaveChangesAsync();
        return quiz;
    }
    
    public async Task<Quiz?> GetQuizByIdAsync(Guid quizId)
    {
        Quiz? quiz = await context.Quizzes.FindAsync(quizId);
        return quiz;
    }
}
