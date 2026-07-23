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
            Title = "Quizz 1 - General Knowledge",
            Description = "Test quiz for general knowledge",
            Status = QuizStatus.published,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };

        context.Quizzes.Add(quiz);

        // Add 15 additional quizzes with unique titles and descriptions for pagination/search testing
        var topics = new[] { "C# Fundamentals", "Angular Signals", "Docker Basics", "SQL Server Tuning", "Clean Code Practices", "Microservices Design", "RabbitMQ Messaging", "Redis Caching", "Entity Framework Core", "Web API Security", "HTML5 & CSS3 Essentials", "TypeScript Advanced", "Git Version Control", "Software Testing", "CI/CD Pipelines" };

        for (int i = 0; i < topics.Length; i++)
        {
            context.Quizzes.Add(new Quiz
            {
                Id = Guid.NewGuid(),
                Title = topics[i],
                Description = $"Complete guide and assessment for {topics[i]}.",
                Status = (i % 3 == 0) ? QuizStatus.draft : QuizStatus.published,
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        await context.SaveChangesAsync();
    }
}
