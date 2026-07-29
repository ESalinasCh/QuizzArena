using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.MatchAttemptUseCases;

public class ResetMatchAttemptUseCase(
    IMatchAttemptRepository matchAttemptRepository,
    IMatchRepository matchRepository
) : IResetMatchAttemptUseCase
{
    public async Task Execute(Guid matchId, Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        Match? match = await matchRepository.GetMatchByIdAsync(matchId) ?? throw new InvalidOperationException("Match doesn't exist");

        List<MatchAttempt> matchAttempts = await matchAttemptRepository.GetAttemptsByUserIds(matchId, [userId]);
        if (matchAttempts.Count == 0)
        {
            throw new InvalidOperationException("User does not have any match attempts.");
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (MatchAttempt matchAttempt in matchAttempts)
        {
            matchAttempt.Deleted = true;
            matchAttempt.DeletedAt = now;
            matchAttempt.UpdatedAt = now;
        }
        await matchAttemptRepository.UpdateMatchAttempts(matchAttempts);
    }
}
