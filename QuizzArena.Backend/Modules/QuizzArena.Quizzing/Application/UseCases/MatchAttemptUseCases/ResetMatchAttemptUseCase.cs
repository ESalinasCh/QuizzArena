using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.MatchAttemptUseCases;

public class ResetMatchAttemptUseCase(
    IMatchAttemptRepository matchAttemptRepository
) : IResetMatchAttemptUseCase
{
    public async Task Execute(Guid userId)
    {
        MatchAttemptFilters filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };
        List<MatchAttempt> matchAttempts = await matchAttemptRepository.GetAttemptsByStudentId(userId, filters);
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
