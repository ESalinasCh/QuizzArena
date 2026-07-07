using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Seeds;
using Shared.Contracts;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence;

internal sealed class DocumentProcessingModuleInitializer : IModuleInitializer
{
    public async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocumentProcessingDbContext>();
        await context.Database.MigrateAsync();
        await SeedRunner.SeedAsync(context);
    }
}
