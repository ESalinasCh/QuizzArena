using QuizzArena.Quizzing.Application.DTOs.QuizAttempt;
using QuizzArena.Quizzing.Application.Ports.In.QuizAttempt;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.QuizAttempt
{
    public class EndQuizAttemptUseCase(IQuizAttemptRepository repository) : IEndQuizAttemptUseCase
    {
        public async Task<EndQuizAttemptResponseDto> Execute(EndQuizAttemptRequestDto dto)
        {
            return new EndQuizAttemptResponseDto();
        }
    }
}
