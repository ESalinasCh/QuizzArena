using Microsoft.EntityFrameworkCore;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;

internal sealed class SqlSeeder
{
    public static async Task SeedAsync(UserDbContext context)
    {
        var scriptPath = Path.Combine(
            AppContext.BaseDirectory,
            "Infrastructure",
            "Adapters",
            "Out",
            "Persistence",
            "Seeds",
            "Sql",
            "UsersSeeder.sql");

        if (!File.Exists(scriptPath))
        {
            return;
        }

        var sql = await File.ReadAllTextAsync(scriptPath);
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
