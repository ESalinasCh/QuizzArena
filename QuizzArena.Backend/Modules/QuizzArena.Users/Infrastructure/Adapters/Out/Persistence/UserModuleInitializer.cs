using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence
{
    internal class UserModuleInitializer : IModuleInitializer
    {
        public void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            context.Database.Migrate();
        }
    }
}