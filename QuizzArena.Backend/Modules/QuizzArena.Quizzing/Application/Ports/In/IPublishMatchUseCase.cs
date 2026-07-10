using QuizzArena.Quizzing.Application.DTOs.Match;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IPublishMatchUseCase
{
    public Task<MatchPublicationResponseDto> Execute(Guid matchId);
}
