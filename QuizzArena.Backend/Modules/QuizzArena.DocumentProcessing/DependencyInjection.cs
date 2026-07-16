using System.Net.Http.Headers;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Application.UseCases;
using QuizzArena.DocumentProcessing.Application.UseCases.ClassSources;
using QuizzArena.DocumentProcessing.Application.Validators;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence.Repositories;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;
using Shared.Contracts;

namespace QuizzArena.DocumentProcessing;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentProcessingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddApplicationPart(typeof(IDocumentProcessingInfrastructureMarker).Assembly);

        services.AddScoped<IUploadSourceUseCase, UploadSourceUseCase>();
        services.AddScoped<IGetClassSourcesUseCase, GetClassSourcesUseCase>();
        services.AddScoped<UploadClassSourceRequestValidator>();
        services.AddScoped<IClassSourceRepository, SqlClassSourceRepository>();
        services.AddScoped<IDocumentChunkRepository, SqlDocumentChunkRepository>();
        services.AddScoped<IProcessingJobRepository, SqlProcessingJobRepository>();

        services.Configure<QuizGenerationOptions>(configuration.GetSection(QuizGenerationOptions.SectionName));
        services.Configure<IndexingOptions>(configuration.GetSection(IndexingOptions.SectionName));

        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);

        services.AddScoped<IStorageServiceRepository, BlobRepository>();
        services.AddSingleton(provider =>
        {
            var cs = configuration.GetConnectionString("AzureBlobStorage")?.Trim();
            if (string.IsNullOrEmpty(cs))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:AzureBlobStorage is not configured. " +
                    "Set it via appsettings or ConnectionStrings__AzureBlobStorage env var.");
            }

            return new BlobServiceClient(cs);
        });

        var transcriptionProvider = configuration["TranscriptionSettings:Provider"];
        var transcriptionBaseUrl = configuration["TranscriptionSettings:BaseUrl"] ?? "http://localhost:9000/";
        if (transcriptionProvider == "Groq")
        {
            /*
                When using Groq:
                - Ensure TranscriptionSettings:ApiKey (user secret or env var) is configured.
                - To add API key to local user secret use: dotnet user-secrets set "TranscriptionSettings:ApiKey" "apikey"
            */
            var apiKey = configuration["TranscriptionSettings:ApiKey"];
            services.AddHttpClient<ITranscriptionService, WhisperGroqTranscription>(client =>
            {
                client.BaseAddress = new Uri(transcriptionBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.Timeout = TimeSpan.FromMinutes(30);
            });
        }
        else
        {
            services.AddHttpClient<ITranscriptionService, WhisperTranscription>(client =>
            {
                client.BaseAddress = new Uri(transcriptionBaseUrl);
                client.Timeout = TimeSpan.FromMinutes(30);
            });
        }


        var textGenerationProvider = configuration["TextGenerationSettings:Provider"];
        var textGenBaseUrl = configuration["TextGenerationSettings:BaseUrl"] ?? "http://localhost:11434/";
        if (textGenerationProvider == "OpenAiApi")
        {
            /*
                When using OpenAiApi (e.g. Groq):
                - Ensure TextGenerationSettings:ApiKey (user secret or env var) is configured.
                - To add API key to local user secret use: dotnet user-secrets set "TextGenerationSettings:ApiKey" "apikey"
            */
            var apiKey = configuration["TextGenerationSettings:ApiKey"];
            services.AddHttpClient<ITextGenerationService, OpenAICompatibleTextGeneration>(client =>
            {
                client.BaseAddress = new Uri(textGenBaseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                client.Timeout = TimeSpan.FromMinutes(60);
            });
        }
        else
        {
            services.AddHttpClient<ITextGenerationService, OllamaTextGeneration>(client =>
            {
                client.BaseAddress = new Uri(textGenBaseUrl);
                client.Timeout = TimeSpan.FromMinutes(60);
            });
        }

        var embeddingProvider = configuration["EmbeddingSettings:Provider"];
        var embeddingBaseUrl = configuration["EmbeddingSettings:BaseUrl"] ?? "http://localhost:11434/";

        if (embeddingProvider == "GoogleGemini")
        {
            /*
                When using GoogleGemini:
                - Ensure EmbeddingSettings:ApiKey (user secret or env var) is configured.
                - To add API key to local user secret use: dotnet user-secrets set "EmbeddingSettings:ApiKey" "apikey"
            */
            services.AddHttpClient<IEmbeddingService, GeminiEmbeddingGeneration>(client =>
            {
                var apiKey = configuration["EmbeddingSettings:ApiKey"];

                client.BaseAddress = new Uri(embeddingBaseUrl);
                client.Timeout = TimeSpan.FromMinutes(60);
                client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
            });
        }
        else
        {
            services.AddHttpClient<IEmbeddingService, OllamaEmbeddingGeneration>(client =>
            {
                client.BaseAddress = new Uri(embeddingBaseUrl);
                client.Timeout = TimeSpan.FromMinutes(60);
            });
        }

        services.AddScoped<IChunkClassifier, OllamaChunkClassifier>();

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
