using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infraestructure.Adapters.Out.Persistence.Repositories
{
    public class SqlQuestionRepository : IQuestionRepository
    {
        public async Task<Question> GetById(Guid id)
        {
            //throw new NotImplementedException();
            return new Question();
        }

        public async Task<Question> Create(Guid id)
        {
            //throw new NotImplementedException();
            return new Question();
        }

        public async Task<Question> Update(Guid id)
        {
            //throw new NotImplementedException();
            return new Question();
        }

        public async Task Delete(Guid id)
        {
            //throw new NotImplementedException();
            return;
        }
    }
}
