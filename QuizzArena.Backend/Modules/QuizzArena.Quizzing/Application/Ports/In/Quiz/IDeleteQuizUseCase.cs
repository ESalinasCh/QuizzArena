using QuizzArena.Quizzing.Application.DTOs.Quiz;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.In.Quiz
{
    public interface IDeleteQuizUseCase
    {
        Task<DeleteQuizResponseDto> Execute(DeleteQuizRequestDto dto);
    }
}
