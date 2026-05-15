using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.Question
{
    internal class CreateQuestion : ICreateQuestion
    {
        private readonly IQuestionRepository _repository;

        public CreateQuestion(IQuestionRepository repository) => _repository = repository;

        public async Task<CreateQuestionResponseDto> Execute(CreateQuizRequestDto dto)
        {
            return new CreateQuestionResponseDto();
        }
    }
}
