using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlMatchAttemptRepository(QuizzingDbContext context) : IMatchAttemptRepository
{
    public async Task<MatchAttempt> AddMatchAttemptAsync(MatchAttempt matchAttempt)
    {
        context.MatchAttempts.Add(matchAttempt);
        await context.SaveChangesAsync();
        return matchAttempt;
    }

    public async Task<int> GetMatchAttemptCountByMatchIdAndUserIdAsync(Guid matchId, Guid userId)
    {
        return await context.MatchAttempts.CountAsync(ma => ma.MatchId == matchId && ma.UserId == userId);
    }

    public async Task<int> GetMatchAttemptCountByMatchIdAndStatusAsync(Guid matchId, QuizAttemptStatus status)
    {
        return await context.MatchAttempts.CountAsync(ma => ma.MatchId == matchId && ma.Status == status);
    }
    public async Task<bool> HasActiveAttemptByMatchIdAsync(Guid matchId, Guid userId)
    {
        return await context.MatchAttempts.AnyAsync(
            ma => ma.MatchId == matchId &&
            ma.UserId == userId &&
            ma.Status == QuizAttemptStatus.InProgress
        );
    }
    public async Task<MatchAttempt?> GetByIdAsync(Guid matchAttemptId)
    {
        return await context.MatchAttempts.FindAsync(matchAttemptId);
    }

    public async Task<MatchAttempt> UpdateAsync(MatchAttempt matchAttempt)
    {
        context.MatchAttempts.Update(matchAttempt);
        await context.SaveChangesAsync();
        return matchAttempt;
    }

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
}
