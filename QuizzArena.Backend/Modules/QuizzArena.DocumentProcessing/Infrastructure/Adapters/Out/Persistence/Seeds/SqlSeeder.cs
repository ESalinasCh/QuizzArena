using Microsoft.EntityFrameworkCore;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Seeds;

internal sealed class SqlSeeder
{
    public static async Task SeedAsync(DocumentProcessingDbContext context)
    {
        if (await context.ProcessingJobs.AnyAsync())
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
            "DocumentProcessingSeeder.sql");

        if (!File.Exists(scriptPath))
        {
            return;
        }

        var sql = await File.ReadAllTextAsync(scriptPath);
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
