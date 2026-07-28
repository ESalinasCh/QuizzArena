using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using Shared.Extensions;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlQuizQueriesRepository(
    QuizzingDbContext context
    ) : IQuizQueriesRepository
{
    public async Task<List<Quiz>> GetByTeacherIdAsync(Guid teacherId, QuizQueryParametersDto query)
    {
        IQueryable<Quiz> q = context.Quizzes
            .AsNoTracking()
            .Where(quiz => quiz.TeacherId == teacherId)
            .Where(quiz => !quiz.Deleted)
            .Include(quiz => quiz.QuizQuestions.Where(qq => !qq.Deleted))
            .ThenInclude(qq => qq.Question);

        if (query.Origin is not null)
        {
            q = q.Where(quiz => quiz.Origin == query.Origin);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            q = q.Where(quiz => quiz.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return await q
            .OrderByDescending(quiz => quiz.CreatedAt)
            .Paginate(query.Page, query.PageSize)
            .ToListAsync();
    }
}
