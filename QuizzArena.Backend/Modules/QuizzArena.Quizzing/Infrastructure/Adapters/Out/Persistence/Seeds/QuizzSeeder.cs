using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;


namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Seeds;

internal sealed class QuizzSeeder
{
    public static async Task SeedAsync(QuizzingDbContext context)
    {
        if (await context.Quizzes.AnyAsync())
        {
            return;
        }

        var quiz = new Quiz
        {
            Id = QuizzingConstants.QuizzId,
            Title = "Quizz 1",
            Description = "Test quiz",
            Status = QuizStatus.published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Quizzes.Add(quiz);

        await context.SaveChangesAsync();
    }
}
