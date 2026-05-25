using QuizzArena.Quizzing.Application.DTOs.QuizAttempt;

namespace QuizzArena.Quizzing.Application.Ports.In.QuizAttempt;

public interface IStartQuizAttemptUseCase
{
    Task<StartQuizAttemptResponseDto> Execute(StartQuizAttemptRequestDto dto);
}
