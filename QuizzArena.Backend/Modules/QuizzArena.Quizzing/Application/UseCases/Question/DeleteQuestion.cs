using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.Question
{
    internal class DeleteQuestion : IDeleteQuestion
    {
        private readonly IQuestionRepository _repository;

        public DeleteQuestion(IQuestionRepository repository) => _repository = repository;
    }
}
