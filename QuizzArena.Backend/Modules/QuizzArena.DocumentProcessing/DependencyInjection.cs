using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence;
using Shared.Contracts;

namespace QuizzArena.DocumentProcessing;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentProcessingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(IDocumentProcessingInfrastructureMarker).Assembly);

        services.AddScoped<IUploadSourceUseCase, UploadSourceUseCase>();

        #region BDD
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();

        dataSourceBuilder.MapEnum<JobStatus>($"{DocumentProcessingConstants.Schema}.job_status");
        dataSourceBuilder.MapEnum<SourceStatus>($"{DocumentProcessingConstants.Schema}.source_status");
        dataSourceBuilder.MapEnum<SourceType>($"{DocumentProcessingConstants.Schema}.source_type");

        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<DocumentProcessingDbContext>(options =>
                options.UseNpgsql(
                    dataSource,
                    o =>
                    {
                        o.MapEnum<JobStatus>(
                            "job_status",
                            DocumentProcessingConstants.Schema
                                );
                        o.MapEnum<SourceStatus>(
                            "source_status",
                            DocumentProcessingConstants.Schema
                            );
                        o.MapEnum<SourceType>(
                            "source_type",
                            DocumentProcessingConstants.Schema
                            );
                    }
                )
            );
        ;

        services.AddTransient<IModuleInitializer, DocumentProcessingModuleInitializer>();
        #endregion

        return services;
    }
}
