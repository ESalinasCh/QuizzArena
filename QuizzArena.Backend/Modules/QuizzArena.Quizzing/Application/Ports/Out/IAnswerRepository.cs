using QuizzArena.Quizzing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.Ports.Out
{
    public interface IAnswerRepository
    {
        Task<Answer> SetAnswer(Guid id);
    }
}
