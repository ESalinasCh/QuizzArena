using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlMatchAttemptRepository(QuizzingDbContext context) : IMatchAttemptRepository
{
    public async Task<MatchAttempt> AddMatchAttemptAsync(MatchAttempt matchAttempt)
    {
        context.MatchAttempts.Add(matchAttempt);
        await context.SaveChangesAsync();
        return matchAttempt;
    }

    public async Task<int> GetMatchAttemptCountByMatchIdAsync(Guid matchId)
    {
        return await context.MatchAttempts.CountAsync(ma => ma.MatchId == matchId);
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
}

