using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Application.UseCases;
using QuizzArena.DocumentProcessing.Application.Validators;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Repositories;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Utils;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;
using Shared.Contracts;

namespace QuizzArena.DocumentProcessing;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentProcessingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddApplicationPart(typeof(IDocumentProcessingInfrastructureMarker).Assembly);

        services.AddScoped<IUploadSourceUseCase, UploadSourceUseCase>();
        services.AddScoped<UploadClassSourceRequestValidator>();
        services.AddScoped<IClassSourceRepository, SqlClassSourceRepository>();
        services.AddScoped<IDocumentChunkRepository, SqlDocumentChunkRepository>();
        services.AddScoped<IProcessingJobRepository, SqlProcessingJobRepository>();
        services.AddScoped<ICosineSimilarity, TensorCosineSimilarity>();

        services.Configure<QuizGenerationOptions>(configuration.GetSection(QuizGenerationOptions.SectionName));

        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);

        services.AddScoped<IStorageServiceRepository, BlobRepository>();
        services.AddSingleton(provider =>
        {
            var storageConnectionString = configuration.GetConnectionString("AzureBlobStorage");
            return new BlobServiceClient(storageConnectionString);
        });

        services.AddHttpClient<ITranscriptionService, WhisperTranscription>(client =>
        {
            var whisperUrl = configuration["WhisperSettings:BaseUrl"] ?? "http://localhost:9000/";
            client.BaseAddress = new Uri(whisperUrl);
            client.Timeout = TimeSpan.FromMinutes(60);
        });

        var ollamaUrl = configuration["OllamaSettings:BaseUrl"] ?? "http://localhost:11434/";
        services.AddHttpClient<ITextGenerationService, OllamaTextGeneration>(client =>
        {
            client.BaseAddress = new Uri(ollamaUrl);
            client.Timeout = TimeSpan.FromMinutes(60);
        });

        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingGeneration>(client =>
        {
            client.BaseAddress = new Uri(ollamaUrl);
            client.Timeout = TimeSpan.FromMinutes(60);
        });

        services.AddHttpClient<IEmbeddingService, EmbeddingService>(client =>
        {
            client.BaseAddress = new Uri(ollamaUrl);
            client.Timeout = TimeSpan.FromMinutes(30);
        });

        services.AddHttpClient<IChunkClassifier, OllamaChunkClassifier>(client =>
        {
            client.BaseAddress = new Uri(ollamaUrl);
            client.Timeout = TimeSpan.FromMinutes(30);
        });

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
