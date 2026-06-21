using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlMatchQueriesRepository(QuizzingDbContext context) : IMatchQueriesRepository
{
    public async Task<List<MatchAttempt>> GetAttemptsByStudentId(Guid studentId, MatchAttemptFilters filters)
    {
        var query = context.MatchAttempts
         .AsNoTracking()
         .Where(x => x.UserId == studentId)
         .Join(
            context.Matches.AsNoTracking(),
            matchAttempt => matchAttempt.MatchId,
            match => match.Id,
            (matchAttempt, match) => new { MatchAttempt = matchAttempt, Match = match }
            ).AsQueryable();

        if (filters.MatchMode.HasValue)
        {
            query = query.Where(x => x.Match.Mode == filters.MatchMode);
        }

        if (filters.MatchId.HasValue)
        {
            query = query.Where(x => x.MatchAttempt.MatchId == filters.MatchId);
        }

        if (filters.Status.HasValue)
        {
            query = query.Where(x => x.MatchAttempt.Status == filters.Status);
        }

        if (filters.MinScore.HasValue)
        {
            query = query.Where(x => x.MatchAttempt.Score >= filters.MinScore);
        }

        if (filters.MaxScore.HasValue)
        {
            query = query.Where(x => x.MatchAttempt.Score <= filters.MaxScore);
        }

        if (filters.StartedFrom.HasValue)
        {
            query = query.Where(x => x.MatchAttempt.StartDateTime >= filters.StartedFrom);
        }

        if (filters.StartedTo.HasValue)
        {
            query = query.Where(x => x.MatchAttempt.StartDateTime <= filters.StartedTo);
        }

        return await query
            .OrderByDescending(x => x.MatchAttempt.StartDateTime)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(x => x.MatchAttempt)
            .ToListAsync();

    }

    public async Task<List<Match>> GetMatchesByIds(List<Guid> matchIds)
    {
        List<Match> matches = await context.Matches.Where(x => matchIds.Contains(x.Id)).ToListAsync();
        return matches;
    }
}
