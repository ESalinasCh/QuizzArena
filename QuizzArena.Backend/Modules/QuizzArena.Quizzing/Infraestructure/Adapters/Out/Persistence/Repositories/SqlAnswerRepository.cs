using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infraestructure.Adapters.Out.Persistence.Repositories
{
    public class SqlAnswerRepository : IAnswerRepository
    {
        public async Task<Answer> SetAnswer(Guid id)
        {
            //throw new NotImplementedException();
            return new Answer();
        }
    }
}
