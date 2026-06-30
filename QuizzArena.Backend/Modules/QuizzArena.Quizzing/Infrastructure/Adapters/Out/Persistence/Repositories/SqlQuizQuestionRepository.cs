using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlQuizQuestionRepository(QuizzingDbContext context) : IQuizQuestionRepository
{
    public async Task CreateMultipleAsync(IEnumerable<QuizQuestion> questions)
    {
        context.QuizQuestions.AddRange(questions);
        await context.SaveChangesAsync();
    }

    public async Task<List<Question>> GetQuestionsByQuizIdAsync(Guid QuizId)
    {
        List<Question> questions = await context.QuizQuestions
            .Where(qq => qq.QuizId == QuizId)
            .Join(context.Questions,
                qq => qq.QuestionId,
                q => q.Id,
                (qq, q) => new { qq, q })
            .OrderBy(x => x.qq.Position)
            .Select(x => x.q)
            .Include(q => q.Options)
            .ToListAsync();

        return questions;
    }

    public async Task<List<AugmentedQuestionDto>> GetQuestionsAndScoreByQuizIdAsync(Guid QuizId)
    {
        List<AugmentedQuestionDto> augmentedQuestions = await context.QuizQuestions
            .Where(qq => qq.QuizId == QuizId)
            .Join(context.Questions,
                qq => qq.QuestionId,
                q => q.Id,
                (qq, q) => new { qq, q })
            .OrderBy(x => x.qq.Position)
            .Select(x => new AugmentedQuestionDto(x.q, x.qq.ValueScore))
            .Include(x => x.Question.Options)
            .ToListAsync();
        return augmentedQuestions;
    }
}
