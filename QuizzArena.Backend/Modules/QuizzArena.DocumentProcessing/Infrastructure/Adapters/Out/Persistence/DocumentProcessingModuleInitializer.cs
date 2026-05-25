using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence
{
    internal class DocumentProcessingModuleInitializer : IModuleInitializer
    {
        public void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DocumentProcessingDbContext>();
            context.Database.Migrate();
        }
    }
}