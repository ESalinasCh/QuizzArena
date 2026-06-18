using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlMatchQueriesRepository(QuizzingDbContext context) : IMatchQueriesRepository
{
    public async Task<List<MatchAttempt>> GetAttemptsByStudentId(Guid studentId, MatchAttemptFilters filters)
    {
        IQueryable<MatchAttempt> query = context.MatchAttempts
         .AsNoTracking()
         .Where(x => x.UserId == studentId);

        if (filters.MatchId.HasValue)
        {
            query = query.Where(x => x.MatchId == filters.MatchId);
        }

        if (filters.Status.HasValue)
        {
            query = query.Where(x => x.Status == filters.Status);
        }

        if (filters.MinScore.HasValue)
        {
            query = query.Where(x => x.Score >= filters.MinScore);
        }

        if (filters.MaxScore.HasValue)
        {
            query = query.Where(x => x.Score <= filters.MaxScore);
        }

        if (filters.StartedFrom.HasValue)
        {
            query = query.Where(x => x.StartDateTime >= filters.StartedFrom);
        }

        if (filters.StartedTo.HasValue)
        {
            query = query.Where(x => x.StartDateTime <= filters.StartedTo);
        }

        return await query
            .OrderByDescending(x => x.StartDateTime)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();
    }

    public async Task<List<Match>> GetMatchesByIds(List<Guid> matchIds)
    {
        List<Match> matches = await context.Matches.Where(x => matchIds.Contains(x.Id)).ToListAsync();
        return matches;
    }
}
