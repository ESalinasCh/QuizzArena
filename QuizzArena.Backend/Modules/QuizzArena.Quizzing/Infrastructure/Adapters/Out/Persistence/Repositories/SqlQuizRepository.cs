using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlQuizRepository(QuizzingDbContext context) : IQuizRepository
{
    public async Task<Quiz?> GetByIdAsync(Guid quizId)
    {
        return await context.Quizzes.FindAsync(quizId);
    }

    public async Task<Quiz> CreateAsync(Quiz quiz)
    {
        context.Quizzes.Add(quiz);
        await context.SaveChangesAsync();
        return await context.Quizzes
            .Include(q => q.QuizQuestions)
            .FirstAsync(q => q.Id == quiz.Id);
    }
}
