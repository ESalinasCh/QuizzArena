using QuizzArena.Quizzing.Application.Ports.In.QuizAttempt;
using QuizzArena.Quizzing.Application.Ports.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.UseCases.QuizAttempt
{
    internal class EndQuizAttempt : IEndQuizAttempt
    {
        private readonly IQuizAttemptRepository _repository;

        public EndQuizAttempt(IQuizAttemptRepository repository) => _repository = repository;
    }
}
