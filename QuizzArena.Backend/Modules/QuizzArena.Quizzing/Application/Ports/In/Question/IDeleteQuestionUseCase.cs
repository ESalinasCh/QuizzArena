using QuizzArena.Quizzing.Application.DTOs.Question;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.In.Question
{
    public interface IDeleteQuestionUseCase
    {
        Task<DeleteQuestionResponseDto> Execute(DeleteQuestionRequestDto dto);
    }
}
