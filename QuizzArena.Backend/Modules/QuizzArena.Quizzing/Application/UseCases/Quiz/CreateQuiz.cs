using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz
{
    internal class CreateQuiz : ICreateQuiz
    {
        private readonly IQuizRepository _repository;

        public CreateQuiz(IQuizRepository repository) => _repository = repository;

        public async Task<CreateQuizResponseDto> Execute(CreateQuizRequestDto dto)
        {
            return new CreateQuizResponseDto();
        }
    }
}
