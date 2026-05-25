using Microsoft.EntityFrameworkCore;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Seeds
{
    internal class SeedRunner
    {
        public static async Task SeedAsync(QuizzingDbContext context)
        {
            await QuizzSeeder.SeedAsync(context);
            await QuestionSeeder.SeedAsync(context);
            await OptionSeeder.SeedAsync(context);
        }
    }
}
