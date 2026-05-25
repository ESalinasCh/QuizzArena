using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Seeds;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence
{
    internal class QuizzingModuleInitializer : IModuleInitializer
    {
        public async void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<QuizzingDbContext>();
            context.Database.Migrate();
            await SeedRunner.SeedAsync(context);
        }
    }
}
