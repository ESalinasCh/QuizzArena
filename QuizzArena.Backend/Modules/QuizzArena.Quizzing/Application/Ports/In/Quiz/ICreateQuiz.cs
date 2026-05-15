using QuizzArena.Quizzing.Application.DTOs.Quiz;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.In.Quiz
{
    internal interface ICreateQuiz
    {
        Task<CreateQuizResponseDto> Execute(CreateQuizRequestDto dto);
    }
}
