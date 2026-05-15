using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz
{
    internal class UpdateQuiz : IUpdateQuiz
    {
        private readonly IQuizRepository _repository;

        public UpdateQuiz(IQuizRepository repository) => _repository = repository;

        public async Task<UpdateQuizResponseDto> Execute(UpdateQuizRequestDto dto)
        {
            return new UpdateQuizResponseDto();
        }
    }
}
