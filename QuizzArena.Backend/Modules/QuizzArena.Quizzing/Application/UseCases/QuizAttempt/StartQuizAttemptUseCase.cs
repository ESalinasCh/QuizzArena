using QuizzArena.Quizzing.Application.DTOs.QuizAttempt;
using QuizzArena.Quizzing.Application.Ports.In.QuizAttempt;
using QuizzArena.Quizzing.Application.Ports.Out;

namespace QuizzArena.Quizzing.Application.UseCases.QuizAttempt;

public class StartQuizAttemptUseCase(IQuizAttemptRepository repository) : IStartQuizAttemptUseCase
{
    public async Task<StartQuizAttemptResponseDto> Execute(StartQuizAttemptRequestDto dto)
    {
        return new StartQuizAttemptResponseDto();
    }
}
