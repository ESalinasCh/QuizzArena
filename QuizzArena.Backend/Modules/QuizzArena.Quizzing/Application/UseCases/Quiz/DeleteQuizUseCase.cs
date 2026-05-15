using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz
{
    public class DeleteQuizUseCase(IQuizRepository repository) : IDeleteQuizUseCase
    {
        public async Task<DeleteQuizResponseDto> Execute(DeleteQuizRequestDto dto)
        {
            return new DeleteQuizResponseDto();
        }
    }
}
