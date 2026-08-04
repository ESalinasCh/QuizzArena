using QuizzArena.Quizzing.Application.DTOs.Match;

namespace QuizzArena.Quizzing.Application.Ports.In.Question;

public interface IUpdateMatchUseCase
{
    Task<MatchUpdatedResponseDto> Execute(Guid matchId, MatchUpdateDto matchUpdateDto);
}
