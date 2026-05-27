using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

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
