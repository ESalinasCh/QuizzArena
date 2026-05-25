using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;
using Shared.Contracts;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence;

internal class UserModuleInitializer : IModuleInitializer
{
    public async void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        context.Database.Migrate();
        await SeedRunner.SeedAsync(context);
    }
}
