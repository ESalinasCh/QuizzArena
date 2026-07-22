using Microsoft.EntityFrameworkCore;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Seeds;

internal sealed class SqlSeeder
{
    public static async Task SeedAsync(QuizzingDbContext context)
    {
        if (await context.Matches.CountAsync() >= 3)
        {
            return;
        }

        var scriptPath = Path.Combine(
            AppContext.BaseDirectory,
            "Infrastructure",
            "Adapters",
            "Out",
            "Persistence",
            "Seeds",
            "Sql",
            "QuizzingSeeder.sql");

        if (!File.Exists(scriptPath))
        {
            return;
        }

        var sql = await File.ReadAllTextAsync(scriptPath);
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
