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
        if (transcriptionProvider == "Groq")
        {

            /*
                Change app.settings file to Groq provider.
                When enabling this registration:
                - Ensure GroqWhisperSettings:ApiKey (user secret) and GroqWhisperSettings:BaseUrl are configured.
                - To add API key to local user secret use: dotnet user-secrets set "GroqWhisperSettings:ApiKey" "apikey"
            */

            var apiKey = configuration["GroqWhisperSettings:ApiKey"];
            var openAIBAseUrl = configuration["GroqWhisperSettings:BaseUrl"] ?? "https://api.groq.com/openai/v1/";

            services.AddHttpClient<ITranscriptionService, WhisperGroqTranscription>(client =>
            {
                client.BaseAddress = new Uri(openAIBAseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.Timeout = TimeSpan.FromMinutes(60);
            });
        }
        else
        {
            var whisperUrl = configuration["WhisperSettings:BaseUrl"] ?? "http://localhost:9000/";

            services.AddHttpClient<ITranscriptionService, WhisperTranscription>(client =>
            {
                client.BaseAddress = new Uri(whisperUrl);
                client.Timeout = TimeSpan.FromMinutes(60);
            });
        }


        var ollamaUrl = configuration["OllamaSettings:BaseUrl"] ?? "http://localhost:11434/";
        var textGenerationProvider = configuration["TextGenerationSettings:Provider"];
        if (textGenerationProvider == "OpenAI")
        {
            /*
                Change app.settings file to OpenAI provider.
                When enabling this registration:
                - Ensure OpenAISettings:ApiKey (user secret) and OpenAISettings:BaseUrl are configured.
                - To add API key to local user secret use: dotnet user-secrets set "OpenAISettings:ApiKey" "apikey"
            */

            var apiKey = configuration["OpenAISettings:ApiKey"];
            var openAIBAseUrl = configuration["OpenAISettings:BaseUrl"] ?? "https://api.groq.com/openai/v1/";
            services.AddHttpClient<ITextGenerationService, OpenAICompatibleTextGeneration>(client =>
            {
                client.BaseAddress = new Uri(openAIBAseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                client.Timeout = TimeSpan.FromMinutes(60);
            });
        }
        else
        {
            services.AddHttpClient<ITextGenerationService, OllamaTextGeneration>(client =>
            {
                client.BaseAddress = new Uri(ollamaUrl);
                client.Timeout = TimeSpan.FromMinutes(60);
            });
        }

        var embeddingProvider = configuration["EmbeddingSettings:Provider"];

        if (embeddingProvider == "Gemini")
        {
            /*
                to use this service, Change EmbeddingSettings:Provider  to Gemini provider.
                When enabling this registration:
                - Ensure GeminiSettings:ApiKey (user secret) and GeminiSettings:BaseUrl are configured.
                - To add API key to local user secret use: dotnet user-secrets set "GeminiSettings:ApiKey" "apikey"
            */
            services.AddHttpClient<IEmbeddingService, GeminiEmbeddingGeneration>(client =>
            {
                var geminiEmbeddingUrl = configuration["GeminiSettings:BaseUrl"]!;
                var apiKey = configuration["GeminiSettings:ApiKey"]!;

                client.BaseAddress = new Uri(geminiEmbeddingUrl);
                client.Timeout = TimeSpan.FromMinutes(60);
                client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
            });
        }
        else
        {
            services.AddHttpClient<IEmbeddingService, OllamaEmbeddingGeneration>(client =>
            {
                client.BaseAddress = new Uri(ollamaUrl);
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
