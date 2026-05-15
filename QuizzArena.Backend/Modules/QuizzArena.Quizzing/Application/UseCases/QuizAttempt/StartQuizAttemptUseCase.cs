using QuizzArena.Quizzing.Application.DTOs.QuizAttempt;
using QuizzArena.Quizzing.Application.Ports.In.QuizAttempt;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.QuizAttempt
{
    public class StartQuizAttemptUseCase(IQuizAttemptRepository repository) : IStartQuizAttemptUseCase
    {
        public async Task<StartQuizAttemptResponseDto> Execute(StartQuizAttemptRequestDto dto)
        {
            return new StartQuizAttemptResponseDto();
        }
    }
}
