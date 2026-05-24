using QuizzArena.Quizzing.Application.DTOs.QuizAttempt;

namespace QuizzArena.Quizzing.Application.Ports.In.QuizAttempt;

public interface IEndQuizAttemptUseCase
{
    Task<EndQuizAttemptResponseDto> Execute(EndQuizAttemptRequestDto dto);
}
