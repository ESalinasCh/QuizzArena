using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlMatchRepository(QuizzingDbContext context) : IMatchRepository
{
    public async Task<List<Match>> GetMatchesAsync(List<Guid> courseIds, MatchQueryParametersDto query)
    {
        IQueryable<Match> q = context.Matches.Where(m => courseIds.Contains(m.CourseId));

        if (query.Code != null)
        {
            q = q.Where(m => m.Code == query.Code);
        }

        if (query.Status != null)
        {
            q = q.Where(m => m.Status == query.Status);
        }

        if (query.Mode != null)
        {
            q = q.Where(m => m.Mode == query.Mode);
        }

        if (query.CourseId != null)
        {
            q = q.Where(m => m.CourseId == query.CourseId);
        }

        if (query.QuizId != null)
        {
            q = q.Where(m => m.QuizId == query.QuizId);
        }

        List<Match> matches = await q.ToListAsync();
        return matches;
    }
}
