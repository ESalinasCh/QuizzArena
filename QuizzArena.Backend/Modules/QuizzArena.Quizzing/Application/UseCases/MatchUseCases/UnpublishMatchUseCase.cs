using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

public class UnpublishMatchUseCase(IMatchRepository matchRepository, IMatchAttemptRepository matchAttemptRepository) : IUnpublishMatchUseCase
{
    public async Task<MatchPublicationResponseDto> Execute(Guid matchId)
    {
        Match? match = await matchRepository.GetMatchByIdAsync(matchId) ?? throw new InvalidOperationException("Match doesn't exist");

        if (match.Status == MatchStatus.Pending)
        {
            throw new InvalidOperationException("Match already is pending");
        }
        int activeAttemptsQuantity = await matchAttemptRepository.GetMatchAttemptCountByMatchIdAndStatusAsync(matchId, QuizAttemptStatus.InProgress);
        if (activeAttemptsQuantity > 0)
        {
            throw new InvalidOperationException("One or more attempts are in progress");
        }

        match.Status = MatchStatus.Pending;

        await matchRepository.UpdateMatchAsync(match);
        return new MatchPublicationResponseDto()
        {
            Id = match.Id,
            PublicationStatus = match.Status,
            StartDate = match.StartedAt,
            EndDate = match.FinishedAt,
            ShareCode = match.Code
        };

    }
}
