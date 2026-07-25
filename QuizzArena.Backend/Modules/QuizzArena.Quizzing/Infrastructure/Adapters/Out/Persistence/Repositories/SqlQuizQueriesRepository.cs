using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlQuizQueriesRepository(
    QuizzingDbContext context
    ) : IQuizQueriesRepository
{
    public async Task<List<Quiz>> GetByTeacherIdAsync(Guid teacherId, QuizOrigin? origin)
    {
        IQueryable<Quiz> query = context.Quizzes
            .AsNoTracking()
            .Where(quiz => quiz.TeacherId == teacherId)
            .Where(quiz => !quiz.Deleted)
            .Include(quiz => quiz.QuizQuestions.Where(qq => !qq.Deleted))
            .ThenInclude(qq => qq.Question);

        if (origin is not null)
        {
            query = query.Where(quiz => quiz.Origin == origin);
        }

        return await query.ToListAsync();
    }
}
