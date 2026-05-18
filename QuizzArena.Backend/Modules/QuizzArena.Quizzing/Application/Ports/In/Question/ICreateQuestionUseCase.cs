using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.In.Question
{
    public interface ICreateQuestionUseCase
    {
        Task<CreateQuestionResponseDto> Execute(CreateQuestionRequestDto dto);
    }
}
