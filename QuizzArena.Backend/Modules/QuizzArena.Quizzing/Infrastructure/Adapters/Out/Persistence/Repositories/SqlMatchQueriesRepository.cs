using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlMatchQueriesRepository(QuizzingDbContext context) : IMatchQueriesRepository
{
    public async Task<List<MatchAttempt>> GetAttemptsByStudentId(Guid studentId)
    {
        List<MatchAttempt> matchAttempts = await context.MatchAttempts.Where(x => x.UserId == studentId).ToListAsync();
        return matchAttempts;
    }

    public async Task<List<Match>> GetMatchesByIds(List<Guid> matchIds)
    {
        List<Match> matches = await context.Matches.Where(x => matchIds.Contains(x.Id)).ToListAsync();
        return matches;
    }
}
