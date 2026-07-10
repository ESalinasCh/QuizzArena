using QuizzArena.Quizzing.Application.DTOs.Match;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IUnpublishMatchUseCase
{
    public Task<MatchPublicationResponseDto> Execute(Guid matchId);
}
