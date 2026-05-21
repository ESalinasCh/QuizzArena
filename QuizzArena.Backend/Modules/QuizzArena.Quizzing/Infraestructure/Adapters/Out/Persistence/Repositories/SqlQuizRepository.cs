using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infraestructure.Adapters.Out.Persistence.Repositories
{
    public class SqlQuizRepository : IQuizRepository
    {
        public async Task<Quiz> GetById(Guid id)
        {
            //throw new NotImplementedException();
            return new Quiz();
        }

        public async Task<Quiz> Create(Guid id)
        {
            //throw new NotImplementedException();
            return new Quiz();
        }

        public async Task<Quiz> Update(Guid id)
        {
            //throw new NotImplementedException();
            return new Quiz();
        }

        public async Task Delete(Guid id)
        {
            //throw new NotImplementedException();
            return;
        }
    }
}
