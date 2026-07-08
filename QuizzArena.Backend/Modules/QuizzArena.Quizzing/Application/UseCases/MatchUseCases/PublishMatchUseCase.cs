using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

internal class PublishMatchUseCase(IMatchRepository matchRepository)
{
    public async Task<MatchPublicationResponseDto> Execute(Guid matchId)
    {
        Match? match = await matchRepository.GetMatchByIdAsync(matchId) ?? throw new InvalidOperationException("Match doesn't exist");

        if (match.Status == MatchStatus.Active)
        {
            throw new InvalidOperationException("Match already is published");
        }
        if (DateTimeOffset.UtcNow >= match.StartedAt)
        {
            throw new InvalidOperationException("StartedAt must be greater than current date");
        }
        if (match.StartedAt >= match.FinishedAt)
        {
            throw new InvalidOperationException("StartedAt must be greater than finished date");
        }

        match.Status = MatchStatus.Active;
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
