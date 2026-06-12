using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IStartAttemptUseCase
{
    Task<StartAttemptResponseDto> Execute(StartAttemptRequestDto request);
}
