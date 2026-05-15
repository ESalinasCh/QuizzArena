using QuizzArena.Quizzing.Application.DTOs.Answer;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.In.Answer
{
    public interface ISetAnswerUseCase
    {
        Task<SetAnswerResponseDto> Execute(SetAnswerRequestDto dto);
    }
}
