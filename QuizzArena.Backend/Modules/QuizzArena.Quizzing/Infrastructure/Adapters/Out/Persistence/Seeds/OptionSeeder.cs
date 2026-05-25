using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Domain.Entities;


namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Seeds;

internal class OptionSeeder
{
    public static async Task SeedAsync(QuizzingDbContext context)
    {
        if (await context.Options.AnyAsync())
        {
            return;
        }

        context.Options.AddRange(
            new Option
            {
                Id = QuizzingConstants.OptionFalse1Id,
                Description = "2",
                IsCorrect = false,
                Position = 0,
                QuestionId = QuizzingConstants.QuestionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Option
            {
                Id = QuizzingConstants.OptionTrueId,
                Description = "4",
                IsCorrect = true,
                Position = 1,
                QuestionId = QuizzingConstants.QuestionId,
                CreatedAt = DateTime.UtcNow
            },
            new Option
            {
                Id = QuizzingConstants.OptionFalse2Id,
                Description = "5",
                IsCorrect = false,
                Position = 2,
                QuestionId = QuizzingConstants.QuestionId,
                CreatedAt = DateTime.UtcNow
            },
            new Option
            {
                Id = QuizzingConstants.OptionFalse3Id,
                Description = "Fish",
                IsCorrect = false,
                Position = 3,
                QuestionId = QuizzingConstants.QuestionId,
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();
    }
}
