namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Seeds;

internal sealed class SeedRunner
{
    public static async Task SeedAsync(DocumentProcessingDbContext context)
    {
        await SqlSeeder.SeedAsync(context);
    }
}

