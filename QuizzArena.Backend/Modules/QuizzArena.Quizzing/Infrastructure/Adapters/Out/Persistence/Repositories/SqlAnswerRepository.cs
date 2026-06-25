using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlAnswerRepository(QuizzingDbContext context) : IAnswerRepository
{
    public async Task<Answer> CreateAnswerAsync(Answer answer)
    {
        await context.AddAsync(answer);
        await context.SaveChangesAsync();

        return answer;
    }
    public async Task<Answer?> GetByAttemptAndQuestionAsync(Guid attemptId, Guid questionId)
    {
        return await context.Answers.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.MatchAttemptId == attemptId);
    }

    public async Task<Answer> UpdateAnswerAsync(Answer answer)
    {
        context.Answers.Update(answer);
        await context.SaveChangesAsync();
        return answer;
    }
}
