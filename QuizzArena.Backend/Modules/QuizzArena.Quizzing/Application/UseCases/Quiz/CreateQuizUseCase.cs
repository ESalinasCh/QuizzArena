using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz
{
    public class CreateQuizUseCase(IQuizRepository repository) : ICreateQuizUseCase
    {
        public async Task<CreateQuizResponseDto> Execute(CreateQuizRequestDto dto)
        {
            return new CreateQuizResponseDto();
        }
    }
}
