using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infraestructure.Adapters.Out
{
    internal class SqlAnswerRepository : IAnswerRepository
    {
        public async Task<Answer> SetAnswer(Guid id)
        {
            //throw new NotImplementedException();
            return new Answer();
        }
    }
}
