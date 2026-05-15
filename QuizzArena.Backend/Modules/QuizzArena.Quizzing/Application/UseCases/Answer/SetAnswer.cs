using QuizzArena.Quizzing.Application.Ports.In.Answer;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.Answer
{
    internal class SetAnswer : ISetAnswer
    {
        private readonly IAnswerRepository _repository;

        public SetAnswer(IAnswerRepository repository) => _repository = repository;
    }
}
