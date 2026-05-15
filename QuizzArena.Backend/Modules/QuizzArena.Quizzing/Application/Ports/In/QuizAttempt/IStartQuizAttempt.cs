using QuizzArena.Quizzing.Application.DTOs.QuizAttempt;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.In.QuizAttempt
{
    internal interface IStartQuizAttempt
    {
        Task<StartQuizAttemptResponseDto> Execute(StartQuizAttemptRequestDto dto);
    }
}
