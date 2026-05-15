using QuizzArena.Quizzing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.Out
{
    internal interface IQuestionRepository
    {
        Task<Question> GetById(Guid id);
        Task<Question> Create(Guid id);
        Task<Question> Update(Guid id);
        Task Delete(Guid id);
    }
}
