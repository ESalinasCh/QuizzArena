using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;
using Shared.Contracts;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence;

internal sealed class UserModuleInitializer : IModuleInitializer
{
    public async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await context.Database.MigrateAsync();
        await SeedRunner.SeedAsync(context);
    }
}
