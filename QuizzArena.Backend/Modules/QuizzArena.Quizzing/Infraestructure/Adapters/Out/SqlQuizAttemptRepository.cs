using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infraestructure.Adapters.Out
{
    internal class SqlQuizAttemptRepository : IQuizAttemptRepository
    {
        public async Task<QuizAttempt> End()
        {
            //throw new NotImplementedException();
            return new QuizAttempt();
        }

        public async Task<QuizAttempt> Start()
        {
            //throw new NotImplementedException();
            return new QuizAttempt();
        }
    }
}
