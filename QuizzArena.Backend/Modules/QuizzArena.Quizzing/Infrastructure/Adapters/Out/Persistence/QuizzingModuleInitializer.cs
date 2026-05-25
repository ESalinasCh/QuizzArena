using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence
{
    internal class QuizzingModuleInitializer : IModuleInitializer
    {
            public void Initialize(IServiceProvider serviceProvider)
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<QuizzingDbContext>();
                context.Database.Migrate();
            }
    }
}
