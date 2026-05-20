using QuizzArena.Quizzing.Application.DTOs.QuizAttempt;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.In.QuizAttempt
{
    public interface IEndQuizAttemptUseCase
    {
        Task<EndQuizAttemptResponseDto> Execute(EndQuizAttemptRequestDto dto);
    }
}
