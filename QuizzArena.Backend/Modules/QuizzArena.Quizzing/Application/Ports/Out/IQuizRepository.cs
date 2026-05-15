using QuizzArena.Quizzing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.Out
{
    internal interface IQuizRepository
    {
        Task<Quiz> GetById(Guid id);
        Task<Quiz> Create(Guid id);
        Task<Quiz> Update(Guid id);
        Task Delete(Guid id);
    }
}
