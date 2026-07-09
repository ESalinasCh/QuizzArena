using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

namespace QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;

public interface IStartAttemptUseCase
{
    Task<StartAttemptResponseDto> Execute(StartAttemptRequestDto request);
}
