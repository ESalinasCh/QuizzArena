using Microsoft.Extensions.DependencyInjection;
using QuizzArena.DocumentProcessing.Application.DTOs.DocumentChunk;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence;

namespace QuizzArena.DocumentProcessing
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDocumentProcessingModule(this IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(IDocumentProcessingInfrastructureMarker).Assembly);

            services.AddScoped<ICreateDocumentUseCase, CreateDocumentUseCase>();
            services.AddScoped<IDocumentChunkRepository, SqlDocumentChunkRepository>();


            // TODO: Add DB Connection

            return services;
        }
    }
}
