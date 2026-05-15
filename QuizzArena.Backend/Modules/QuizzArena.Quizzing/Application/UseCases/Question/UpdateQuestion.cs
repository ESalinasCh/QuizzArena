using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.Question
{
    internal class UpdateQuestion : IUpdateQuestion
    {
        private readonly IQuestionRepository _repository;

        public UpdateQuestion(IQuestionRepository repository) => _repository = repository;

        public async Task<UpdateQuestionResponseDto> Execute(UpdateQuestionRequestDto dto)
        {
            return new UpdateQuestionResponseDto();
        }
    }
}
