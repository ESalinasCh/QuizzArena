using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;


namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Seeds;

internal class QuestionSeeder
{
    public static async Task SeedAsync(QuizzingDbContext context)
    {
        if (await context.Questions.AnyAsync())
        {
            return;
        }

        var question = new Question
        {
            Id = QuizzingConstants.QuestionId,
            Content = "What is 2+2?",
            Status = QuestionStatus.Verified,
            Type = QuestionType.SingleChoice,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Questions.Add(question);

        await context.SaveChangesAsync();
    }
}
