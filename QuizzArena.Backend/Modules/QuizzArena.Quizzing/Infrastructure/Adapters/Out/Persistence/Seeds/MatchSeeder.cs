using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Seeds
{
    internal class MatchSeeder
    {
        public static async Task SeedAsync(QuizzingDbContext context)
        {
            if (await context.Matches.AnyAsync())
                return;

            var quiz = new Match
            {
                Id = QuizzingConstants.MatchId,
                Code = "1234",
                Status = MatchStatus.Active,
                Mode = MatchMode.Single,
                StartedAt = DateTime.UtcNow,
                TimeMinutes = 30,
                Deleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CourseId = QuizzingConstants.CourseId,
                QuizId = QuizzingConstants.QuizzId
            };

            context.Matches.Add(quiz);

            await context.SaveChangesAsync();
        }
    }
}
