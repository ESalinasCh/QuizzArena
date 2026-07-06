using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlMatchRepository(QuizzingDbContext context) : IMatchRepository
{
    public async Task<Match?> GetMatchByIdAsync(Guid matchId)
    {
        return await context.Matches.FindAsync(matchId);
    }

    public async Task<List<Match>> GetMatchesByIds(List<Guid> matchIds)
    {
        List<Match> matches = await context.Matches.Where(x => matchIds.Contains(x.Id)).ToListAsync();
        return matches;
    }

    public async Task<List<Match>> GetMatchesAsync(List<Guid> courseIds, MatchQueryParametersDto? query = null)
    {
        IQueryable<Match> q = context.Matches.Where(m => courseIds.Contains(m.CourseId));

        if (query == null)
        {
            return await q.ToListAsync();
        }

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

        return await q.ToListAsync();
    }
    public async Task<MatchAttempt?> GetMatchAttemptsDetailById(Guid matchAttemptId)
    {
        return await context.MatchAttempts.Include(x => x.Answers).Include(x => x.MatchAttemptQuestions).FirstOrDefaultAsync(x => x.Id == matchAttemptId);
    }

    public async Task<Match> CreateMatchAsync(Match match)
    {
        var entry = await context.Matches.AddAsync(match);
        await context.SaveChangesAsync();
        return entry.Entity;
    }
}
