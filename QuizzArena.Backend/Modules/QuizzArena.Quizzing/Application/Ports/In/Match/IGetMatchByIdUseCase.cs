using QuizzArena.Quizzing.Application.DTOs.Match;

namespace QuizzArena.Quizzing.Application.Ports.In.Match;

public interface IGetMatchByIdUseCase
{
    Task<MatchDetailResponseDto> Execute(Guid matchId);
}
