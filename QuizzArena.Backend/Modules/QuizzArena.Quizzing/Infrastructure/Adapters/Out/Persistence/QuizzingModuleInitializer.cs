using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Seeds;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;

internal class QuizzingModuleInitializer : IModuleInitializer
{
    public async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuizzingDbContext>();
        await context.Database.MigrateAsync();
        await SeedRunner.SeedAsync(context);
    }
}
