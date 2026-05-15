using QuizzArena.Quizzing.Application.DTOs.Question;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.In.Question
{
    internal interface IUpdateQuestion
    {
        Task<UpdateQuestionResponseDto> Execute(UpdateQuestionRequestDto dto);
    }
}
