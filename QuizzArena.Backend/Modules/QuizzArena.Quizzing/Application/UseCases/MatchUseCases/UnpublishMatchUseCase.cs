using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

internal class UnpublishMatchUseCase(IMatchRepository matchRepository)
{
    public async Task<List<MatchResponseDto>> Execute(Guid matchId)
    {
        Match? match = await matchRepository.GetMatchByIdAsync(matchId) ?? throw new InvalidOperationException("Match doesn't exist");

        if (match.Status == MatchStatus.Pending)
        {
            throw new InvalidOperationException("Match already is pending");
        }
        //more valdiations, match attempts active count, mm something else? etc


        match.Status = MatchStatus.Pending;

        await matchRepository.UpdateMatchAsync(match);

    }
}
