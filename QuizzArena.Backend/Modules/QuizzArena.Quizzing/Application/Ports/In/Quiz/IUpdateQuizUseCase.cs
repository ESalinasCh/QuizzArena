using QuizzArena.Quizzing.Application.DTOs.Quiz;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.In.Quiz
{
    public interface IUpdateQuizUseCase
    {
        Task<UpdateQuizResponseDto> Execute(UpdateQuizRequestDto dto);
    }
}
